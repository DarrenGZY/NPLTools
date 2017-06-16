local socket = require"socket"
local lfs = require"lfs"
local debug = require"debug"
local json = require"json"

-- current 
local system_file = nil

local coro_debugger
local events = { BREAK = 1, WATCH = 2, MODULELOAD = 3 }
local breakpoints = {}
local watches = {}
local step_into = false
local step_over = false
local step_level = 0
local stack_level = 0
local cur_frame = { parent = {} }	

local responses = {
	BreakpointHit = tostring(201),
	ModuleLoad = tostring(202)
}
-- modules in the application
local Modules = {}
-- number of modules loaded
local modulesCounter = 0;
-- threads in the application
local Threads = {}
-- number of threads 
local threadsCounter = 0;
-- if the thread is created
local isThreadCreated = false;
-- number of breakpoints, should be sync as _breakpointCount in LuaProcess.cs
local breakpointCounter = 0;

local function set_breakpoint(file, line)
	if not breakpoints[file] then
		breakpoints[file] = {}
	end
	breakpoints[file][line] = breakpointCounter
	breakpointCounter = breakpointCounter + 1
end

local function remove_breakpoint(file, line)
	if breakpoints[file] then
		breakpoints[file][line] = nil
	end
end

local function has_breakpoint(file, line)
	return breakpoints[file] and (breakpoints[file][line] ~= nil)
end

local function get_breakpointId(file, line)
	return breakpoints[file][line]
end

local function table_copy(obj)
  if type(obj) ~= 'table' then return obj end
  local res = {}
  for k, v in pairs(obj) do res[table_copy(k)] = table_copy(v) end
  return res
end

local function restore_vars(vars)
	if type(vars) ~= 'table' then return end
	local func = debug.getinfo(3, "f").func
	local i = 1
	local written_vars = {}
	while true do
		local name = debug.getlocal(3, i)
		if not name then break end
		debug.setlocal(3, i, vars[name])
		written_vars[name] = true
		i = i + 1
	end
	i = 1
	while true do
		local name = debug.getupvalue(func, i)
		if not name then break end
		if not written_vars[name] then
			debug.setupvalue(func, i, vars[name])
			written_vars[name] = true
		end
		i = i + 1
	end
end

local function capture_vars()
	local vars = {}
	local func = debug.getinfo(3, "f").func
	local i = 1
	while true do
		local name, value = debug.getupvalue(func, i)
		if not name then break end
		vars[name] = value
		i = i + 1
	end
	i = 1
	while true do
		local name, value = debug.getlocal(3, i)
		if not name then break end
		vars[name] = value
		i = i + 1
	end
	setmetatable(vars, { __index = getfenv(func), __newindex = getfenv(func) })
	return vars
end

local function break_dir(path) 
	local paths = {}
	path = string.gsub(path, "\\", "/")
	for w in string.gfind(path, "[^\/]+") do
		table.insert(paths, w)
	end
	return paths
end

local function get_filename(path)
	local paths = break_dir(path)
	return paths[#paths]
end

local function merge_paths(path1, path2)
	local paths1 = break_dir(path1)
	local paths2 = break_dir(path2)
	for i, path in ipairs(paths2) do
		if path == ".." then
			table.remove(paths1, table.getn(paths1))
		elseif path ~= "." then
			table.insert(paths1, path)
		end
	end
	return table.concat(paths1, "/")
end

local function dumpInfo(event)
	local info = debug.getinfo(3, "Sl")
	--print(info.what)
	print(string.format("[%s]: %s %d '%s'", event, info.short_src, info.currentline, info.what))
end

local function dumpFrameInfo(frame)
	if frame.filename and frame.lineNo then
		print(string.format("[%s]: %d", frame.filename, frame.lineNo))
	end
end

local function dumpVarInfo(vars)
	for k, v in pairs(vars) do
		print(k, v)
	end
end

local function debug_hook(event, line)
	--socket.sleep(5)
	if event == "call" then
		stack_level = stack_level + 1
		dumpInfo("call")
		local info = debug.getinfo(2, "Sl")
		if info.what == "Lua" then
			dumpFrameInfo(cur_frame)
			local tmp_frame = table_copy(cur_frame)
			tmp_frame.parent = cur_frame
			cur_frame = tmp_frame
		end
	elseif event == "return" then
		stack_level = stack_level - 1
		dumpInfo("return")
		local info = debug.getinfo(2, "Sl")
		if info.what == "Lua" then
			dumpFrameInfo(cur_frame)
			cur_frame = table_copy(cur_frame.parent)
		end
	else
		dumpInfo("line")
		local file = debug.getinfo(2, "S").source
		if string.find(file, "@") == 1 then
			file = string.sub(file, 2)
		end
		
		-- set the frame
		cur_frame.lineNo = line
		cur_frame.filename = file
		
		local vars = capture_vars()
		dumpVarInfo(vars);
		--print(get_filename(file))
		--print(lfs.currentdir().."visualstudio_lua_debugger.lua")
		print("before module load attempt")
		--socket.sleep(10)
		
		if Modules[file] == nil and get_filename(file) ~= "visualstudio_lua_debugger.lua" then
			print("module loading...")
			local id = modulesCounter
			Modules[file] = id
			modulesCounter = modulesCounter + 1
			coroutine.resume(coro_debugger, events.MODULELOAD, vars, file, line, id)
			restore_vars(vars)
		end
		
		print("after module load attempt")
		--socket.sleep(10)
		table.foreach(watches, function (index, value)
			setfenv(value, vars)
			local status, res = pcall(value)
			if status and res then
				coroutine.resume(coro_debugger, events.WATCH, vars, file, line, index)
				restore_vars(vars)
			end
		end)
		
		if step_into or (step_over and stack_level <= step_level)then
			step_into = false
			step_over = false
			coroutine.resume(coro_debugger, events.BREAK, vars, file, line)
			restore_vars(vars)
		end
		
		if has_breakpoint(file, line) then
			local id = get_breakpointId(file, line)
			coroutine.resume(coro_debugger, events.BREAK, vars, files, line, id)
		end
	end
end

local function debugger_loop(server)
	local function Send_BreakpointHitEvent(id)
		print("send breakpoint hit")
		--socket.sleep(10)
		local msg = {
			name = "BreakpointHit",
			id = id,
		}
		server:send(json.encode(msg).."\n")
	end
	
	local function Send_StepCompleteEvent()
		print("send step complete")
		--socket.sleep(10)
		local msg = {
			name = "StepComplete",
		}
		server:send(json.encode(msg).."\n")
	end
	
	local function Send_FrameList()
		print("send frame list")
		
		local msg = {
			name = "FrameList",
			frame = cur_frame
		}
		print(json.encode(msg))
		server:send(json.encode(msg).."\n")
	end
	
	local function Send_ModuleLoad(file, id)
		print("send module load")
		--socket.sleep(10)
		local msg = {
			name = "ModuleLoad",
			filename = file,
			id = id
		}
		server:send(json.encode(msg).."\n")
	end
	
	local function Send_ThreadCreate()
		print("send thread create")
		--socket.sleep(10)
		local msg = {
			name = "ThreadCreate"
		}
		server:send(json.encode(msg).."\n")
	end
	
	local command
	local eval_env = {}
	local continue_running= false
	
	local first_launch, second_launch = true, false
	
	while true do
		print("in debug loop: waiting for command...")
		if first_launch then 
			while true do
				local line, status
				server:settimeout(5)
				line, status = server:receive()
				if status == "timeout" then
					print("timeout")
					break
				end
				command = string.sub(line, string.find(line, "^[A-Z]+"))
				if command == "SETB" then
					local _, _, _, filename, line = string.find(line, "^([A-Z]+)%s+([%w%p]+)%s+(%d+)$")
					if filename and line then
						filename = string.gsub(filename, "%%20", " ")
						set_breakpoint(filename, tonumber(line))
					end
				end
			end
			server:settimeout(-1)
			first_launch = false
			second_launch = true
		end
		
		local line, status
		if second_launch then
			line = "RUN"
			second_launch = false
			print("second launch...")
		else
			print("normal waiting...")
			if continue_running then
				line = "RUN"
				continue_running = false
			else
				line, status = server:receive()
			end
		end
		
		print(line)
		command = string.sub(line, string.find(line, "^[A-Z]+"))
		print("command: "..command)
		if command == "SETB" then
			local _, _, _, filename, line = string.find(line, "^([A-Z]+)%s+([%w%p]+)%s+(%d+)$")
			if filename and line then
				filename = string.gsub(filename, "%%20", " ")
				set_breakpoint(filename, tonumber(line))
				--server:send("200 OK\n")
			else
				--server:send("400 Bad Request\n")
			end
		elseif command == "DELB" then
			local _, _, _, filename, line = string.find(line, "^([A-Z]+)%s+([%w%p]+)%s+(%d+)$")
			if filename and line then
				remove_breakpoint(filename, tonumber(line))
				--server:send("200 OK\n")
			else
				--server:send("400 Bad Request\n")
			end
		elseif command == "EXEC" then
			local _, _, chunk = string.find(line, "^[A-Z]+%s+(.+)$")
			if chunk then 
				local func = loadstring(chunk)
				local status, res
				if func then
					setfenv(func, eval_env)
					status, res = xpcall(func, debug.traceback)
				end
				res = tostring(res)
				if status then
					--server:send("200 OK " .. string.len(res) .. "\n") 
					--server:send(res)
				else
					--server:send("401 Error in Expression " .. string.len(res) .. "\n")
					--server:send(res)
				end
			else
				server:send("400 Bad Request\n")
			end
		elseif command == "SETW" then
			local _, _, exp = string.find(line, "^[A-Z]+%s+(.+)$")
			if exp then 
				local func = loadstring("return(" .. exp .. ")")
				local newidx = table.getn(watches) + 1
				watches[newidx] = func
				table.setn(watches, newidx)
				--server:send("200 OK " .. newidx .. "\n") 
			else
				--server:send("400 Bad Request\n")
			end
		elseif command == "DELW" then
			local _, _, index = string.find(line, "^[A-Z]+%s+(%d+)$")
			index = tonumber(index)
			if index then
				watches[index] = nil
				--server:send("200 OK\n") 
			else
				--server:send("400 Bad Request\n")
			end
		elseif command == "RUN" then
			print("run command received")
			--server:send("200 OK\n")
			local ev, vars, file, line, idx = coroutine.yield()
			print("running...")
			eval_env = vars
			if ev == events.BREAK then
				Send_FrameList()
				Send_BreakpointHitEvent(idx)
			elseif ev == events.MODULELOAD then
				print("prapering to send module load event...")
				Send_ModuleLoad(file, idx)
				if (not isThreadCreated) then
					Send_ThreadCreate()
					isThreadCreated = true
				end
				continue_running = true
			elseif ev == events.WATCH then
				--server:send("203 Paused " .. file .. " " .. line .. " " .. idx .. "\n")
			else
				--server:send("401 Error in Execution " .. string.len(file) .. "\n")
				server:send(file)
			end
		elseif command == "STEP" then
			--server:send("200 OK\n")
			step_into = true
			local ev, vars, file, line, idx = coroutine.yield()
			eval_env = vars
			if ev == events.BREAK then
				Send_FrameList()
				Send_StepCompleteEvent()
			elseif ev == events.WATCH then
				--server:send("203 Paused " .. file .. " " .. line .. " " .. idx_watch .. "\n")
			else
				--server:send("401 Error in Execution " .. string.len(file) .. "\n")
				server:send(file)
			end
		elseif command == "OVER" then
			--server:send("200 OK\n")
			step_over = true
			step_level = stack_level
			local ev, vars, file, line, idx= coroutine.yield()
			eval_env = vars
			if ev == events.BREAK then
				Send_FrameList()
				Send_StepCompleteEvent()
				--server:send("202 Paused " .. file .. " " .. line .. "\n")
			elseif ev == events.WATCH then
				--server:send("203 Paused " .. file .. " " .. line .. " " .. idx_watch .. "\n")
			else
				--server:send("401 Error in Execution " .. string.len(file) .. "\n")
				server:send(file)
			end
		else
			--server:send("400 Bad Request\n")
		end
	end
end

coro_debugger = coroutine.create(debugger_loop)

function startDebug(file, port)
	local server = socket.connect("localhost", port)
	if server then
		_TRACEBACK = function (message) 
			local err = debug.traceback(message)
			--server:send("401 Error in Execution " .. string.len(err) .. "\n")
			server:send(err)
			server:close()
			return err
		end
		debug.sethook(debug_hook, "lcr")
		coroutine.resume(coro_debugger, server)
		dofile(file)
	end
end

local file = arg[1]
local port = arg[2]

startDebug(file, port);
socket.sleep(120);
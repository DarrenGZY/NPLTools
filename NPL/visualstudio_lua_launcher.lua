print("launching the lua debuger ...")

local vsld = require("visualstudio_lua_debugger")

if arg[1] == nil or arg[2] == nil then
	return;
end

local filename = arg[1];
local portnum = arg[2];

vsld.debug(filename, portnum);
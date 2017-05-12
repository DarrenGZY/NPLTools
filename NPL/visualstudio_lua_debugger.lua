local socket = require("socket")

local debugger = {}


function debugger.debug(filename, portnum)
	print("start debugging on port " .. portnum)
	
	local client = assert(socket.connect("localhost", portnum));
	--socket.sleep(5);
	--client:send("hand shake from lua<EOF>");
	while true do
		--local msg = client:receive();
		-- print(msg);
		socket.sleep(5);
		client:send("hello from lua");
	end
end

return debugger;
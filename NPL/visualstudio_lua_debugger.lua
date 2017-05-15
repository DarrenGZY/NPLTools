local socket = require("socket")

local debugger = {}


function debugger.debug(filename, portnum)
	print("start debugging on port " .. portnum)
	
	local client = assert(socket.connect("localhost", portnum));
	--socket.sleep(5);
	socket.sleep(20);
	client:send("moduleload\n");
	while true do
		--local msg = client:receive();
		-- print(msg);
		socket.sleep(5);
		local msg = client:receive();
		print(msg)
	end
end

return debugger;
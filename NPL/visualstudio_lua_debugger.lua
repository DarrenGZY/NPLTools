local socket = require("socket")

local debugger = {}


function debugger.start(filename, portnum)
	print("start debugging ......")

	local client = socket.connect("localhost", 11111);

	print(client)
	
	
	local server = socket.bind("localhost", 8171);
	local client = server:accept();
end

return debugger;
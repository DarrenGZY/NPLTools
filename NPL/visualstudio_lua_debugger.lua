local socket = require("socket")

local debugger = {}


function debugger.debug(filename, portnum)
	print("start debugging ......")

	local client = socket.connect("localhost", portnum);
	client:send("hand shake from lua<EOF>");
	while true do
		local msg = client:receive();
		print(msg);
		socket.sleep(5);
		client:send("hello from lua<EOF>");
	end
end

return debugger;
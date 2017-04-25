local request = NPL.load('./request.lua');
local router = NPL.load('./router.lua');

local handler = {};



local function activate()
	local req = request:new(msg);
	router.match(req);
end


NPL.this(activate);

--return handler;
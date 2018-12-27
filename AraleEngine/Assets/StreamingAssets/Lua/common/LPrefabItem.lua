if not LPrefabItem then

LPrefabItem = 
{
	b="b";
	mPrefabItem = nil;
}

function LPrefabItem:new(csobj)
	print("LPrefabItem");
	csobj.luaAwake = function() self:Awake(); end
end

function LPrefabItem:Awake()
	print("Awake:"..self.test..self.Cube.name);
end

function LPrefabItem:regEventListener(ctrlPath, ctrlId)
	self.mPrefabItem:RegCtrlEventListener(ctrlPath,ctrlId);
end
--========================	
createClass("LPrefabItem",LPrefabItem,LObject)
end
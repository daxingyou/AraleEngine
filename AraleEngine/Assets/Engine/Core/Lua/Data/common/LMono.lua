if not LMono then

LMono = 
{
	val = "LMono";
}

function LMono:new(csobj)
	csobj.luaOnStart = self.onStart
	csobj.luaOnDestroy = self.onDestroy
end

function LMono:onStart()
	print("start:"..self.val)
end

function LMono:onDestroy()
	print("destroy:"..self.val)
end
--========================	
createClass("LMono",LMono,LObject)
end
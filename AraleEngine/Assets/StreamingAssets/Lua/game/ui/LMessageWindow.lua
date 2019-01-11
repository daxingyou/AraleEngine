if not LMessageWindow then
--======================
local M=
{
	_csg;
	_onYes;
	_onNo;
}

function M:new(cs)
	self._csg = cs.gameObject
	cs.luaOnEnable = function() self:OnEnable(); end
end

function M:OnEnable()
	self.luaNo:SetActive(false)
	self.luaTip = self.luaTip:GetComponent("Text");
	EventListener.Get(self.luaYes):AddOnClick(function(evt)  if self._onYes~= nil then self._onYes()end;GameObject.Destroy(self._csg) end)
	EventListener.Get(self.luaNo):AddOnClick(function(evt)  if self._onNo~= nil then self._onNo()end;GameObject.Destroy(self._csg) end)
end

function M:ShowNo(nocall,noname)
	self.luaNo:SetActive(true)
	self._onNo = nocall;
	if noname~= nil then self.luaNo:GetComponentInChildren("Text").text = noname end
	return self;
end

function M:ShowMessage(msg,yescall,yesname)
	self._onYes = yescall;
	if yesname~= nil then self.luaYes:GetComponentInChildren("Text").text = yesname end
	self.luaTip.text= msg;
	return self;
end
--=======================
LMessageWindow = M
createClass("LMessageWindow",LMessageWindow);
end
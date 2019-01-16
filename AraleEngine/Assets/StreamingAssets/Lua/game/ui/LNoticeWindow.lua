if not LNoticeWindow then print("same lua, reload ignore!!!") end
--======================
local M=
{
	_cs;
}

function M:new(cs)
	self._cs = cs;
	cs.luaOnAwake = self.Awake
end

function M:Awake()
end

function M:ShowNotice(notice)
	local tip = GameObject.Instantiate(self.luaTip):GetComponent(typeof(UI.Text))
	tip.gameObject:SetActive(true)
	tip.transform:SetParent(self._cs.transform, false)
	tip.text = notice;
	tip.transform:DOLocalMoveY(100,1)
	GameObject.Destroy(tip.gameObject,1.5)
	WindowMgr.single:BringToTop(self._cs)
end

--=======================
LNoticeWindow = M
createClass("LNoticeWindow",LNoticeWindow,LWindow);
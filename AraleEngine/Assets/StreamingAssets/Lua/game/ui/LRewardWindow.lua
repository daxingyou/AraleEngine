if not LRewardWindow then

local M =
{
	_cs;
	_skill;
}

function M:new(cs)
	self._cs = cs
	cs.luaOnAwake = self.Awake
end

function M:Awake()
	self.luaItems = self.luaItems:GetComponent("UISList")
end

function M:ShowRewards(rewards)
	local list = self.luaItems
	list:clearItem()
	local seq = DOTween.Sequence()
	local time = 0
	for i=1,rewards.Count,2 do
		local id = rewards[i-1]
		local num= rewards[i]
		local item = LTBItem[id]
		if item ~= nil then
			local it = list:addItem(0)
			local lit = it.mLO.mLT
			AssetRef.setImage(lit.luaIcon:GetComponent("Image"), item.icon)
			lit.luaName:GetComponent("Text").text = tostring(item.name)
			lit.luaNum:GetComponent("Text").text = tostring(num)

			local t = it.transform
			t:DOScale(0.5, 1):Complete()
			seq:Insert(time, t:DOScale(1, 0.3))
			time=time+0.2
		end
	end
	seq:LPlay():LOnComplete(function() GameObject.Destroy(self._cs.gameObject,1) end)
end
--========================
LRewardWindow = M
function LRewardWindow.Show(rewards)--rewards为List<int>，偶数位id，奇数位num
	print(rewards.Count)
	if rewards.Count < 1 then return end
	local win = WindowMgr.single:GetWindow("RewardWindow", true).mLO.mLT
	win:ShowRewards(rewards)
end
createClass("LRewardWindow",LRewardWindow)
end
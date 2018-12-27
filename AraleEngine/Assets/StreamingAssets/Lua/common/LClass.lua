if not LClass then

--code imp class inherit--
function newLuaObject(className,...)
	--print("newLuaObject:"..className);
	if nil==className then print("newLuaObject c# object is null"); end
	local f=load("return instantiate("..className..",...)");
	return f(...);--call instantiate
end

function instantiate(a,...)
	if not a then
		error("new a invalid Class");
		return nil
	end

	local result = {__index=result;test="test"};
	setmetatable(result,a);
	result:___new(...);
	return result;
end

function createClass(subClassName,subClass,baseClass)
	if  subClass == nil then
		error("createClass: subClass is nil");
	end

	subClass.__index = subClass;
	subClass.___type = subClassName;
	if nil~=baseClass then
		setmetatable(subClass,baseClass);
		subClass[baseClass.___type] = baseClass;
		subClass.___new = function(self,...)
			self[baseClass.___type].___new(self,...);
			--call class func and use object data
			if nil~=subClass.new then subClass.new(self,...); end
		end
	else
		subClass.___new = function(self,...)
			if nil~=subClass.new then subClass.new(self,...); end
		end
	end
	return subClass;
end

end

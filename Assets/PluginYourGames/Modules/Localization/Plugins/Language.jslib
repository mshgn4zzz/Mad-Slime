mergeInto(LibraryManager.library,
{
	LangRequest_js: function ()
	{
		var returnStr = navigator.language || navigator.userLanguage || "en";

		if (ysdk !== null && ysdk.environment && ysdk.environment.i18n && ysdk.environment.i18n.lang) {
			returnStr = ysdk.environment.i18n.lang;
		}
		else {
			returnStr = returnStr.split('-')[0];
		}

		var bufferSize = lengthBytesUTF8(returnStr) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(returnStr, buffer, bufferSize);
		return buffer;
	},

	GeneralLanguage_js: function ()
	{
		var returnStr = navigator.language || navigator.userLanguage || "en";
		var bufferSize = lengthBytesUTF8(returnStr) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(returnStr, buffer, bufferSize);
		return buffer;
	}
});
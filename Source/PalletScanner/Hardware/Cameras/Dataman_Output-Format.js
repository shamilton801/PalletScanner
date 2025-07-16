function getHeaderInfo(readerProperties) {
	return [
		readerProperties.name,
		readerProperties.trigger.creationTime
	];
}
function getInfoPerResult(decodeResult) {
	var corners = decodeResult.symbology.corners;
	var cornerString = corners.map(function(corner) {
		return corner.x + "," + corner.y;
	}).join("\n");
	return [
		decodeResult.symbology.center.x,
		decodeResult.symbology.center.y,
		decodeResult.symbology.angle,
		decodeResult.symbology.name,
		decodeResult.symbology.moduleSize,
		encode_base64(cornerString),
		encode_base64(decodeResult.content)
	];
}
var VersionNumber = "2.0"; // NOTE: UPDATE THIS WHENEVER THE OUTPUT FORMAT CHANGES!!!

function FormatOutput(headerInfo, bodyInfo) {
	var lines = [
		["GateKeeper Reader Format " + VersionNumber],
		headerInfo
	].concat(bodyInfo);
	return lines.map(function(line) {
		return line.join(",");
	}).join("\n");
}
function onResult(decodeResults, readerProperties, output) {
	var headerInfo = getHeaderInfo(readerProperties);
	var bodyInfo = [];
	for(var i = 0; i < decodeResults.length; i++) {
		var decodeResult = decodeResults[i];
		if (!decodeResult.decoded) continue;
		bodyInfo.push(getInfoPerResult(decodeResult));
	}
	output.content = FormatOutput(headerInfo, bodyInfo) + "\r";
}
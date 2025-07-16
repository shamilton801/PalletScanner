var name = dmccGet("DEVICE.NAME").response;

function onResult(decodeResults, readerProperties, output) {
	var s = name;
	s += ",";
	s += readerProperties.trigger.creationTime;	// Does not change

	s += "\n";
	if (decodeResults[0].decoded) {

		for (var i = 0; i < decodeResults.length; i++) {
			if (!decodeResults[i].decoded) continue;
			if (decodeResults[i].source != name) continue;
			if (decodeResults[i].decoded) {
				s += decodeResults[i].symbology.center.x;
				s += ",";
				s += decodeResults[i].symbology.center.y;
				s += ",";
				s += decodeResults[i].symbology.angle;
				s += ",";
				s += decodeResults[i].symbology.name;
				s += ",";
				s += decodeResults[i].symbology.moduleSize;
				s += ",";
				s += decodeResults[i].content;
				if (i < decodeResults.length - 1) {
					s += "\n";
				}
			}
		}
		s += "\r";
		output.content = s;
	}

}
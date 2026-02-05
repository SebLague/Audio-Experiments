float dstToLineSegment(float2 p, float2 lineStart, float2 lineEnd) {
	float2 aB = lineEnd - lineStart;
	float2 aP = p - lineStart;
	float sqrLenAB = dot(aB, aB);

	if (sqrLenAB == 0)
	{
		return length(aP);
	}

	float t = saturate(dot(aP, aB) / sqrLenAB);
	float2 closestPoint = lineStart + aB * t;
	float dstFromLine = length(closestPoint - p);
	return dstFromLine;
}

float2 drawLineDstInfo(float2 p, float2 lineStart, float2 lineEnd) {
	float2 aB = lineEnd - lineStart;
	float2 aP = p - lineStart;
	float sqrLenAB = dot(aB, aB);

	if (sqrLenAB == 0)
	{
		return float2(length(aP), 1);
	}

	float t = saturate(dot(aP, aB) / sqrLenAB);
	float2 closestPoint = lineStart + aB * t;
	float dstFromLine = length(closestPoint - p);
	return float2(dstFromLine, t);
}

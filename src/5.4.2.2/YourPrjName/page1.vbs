Set poster = CreateObject("Microsoft.XMLHTTP")
Set XMLDocument=CreateObject("Microsoft.XMLDOM")
sub update()
poster.Open "GET", "page1.xml", False
poster.send
XMLDocument.async=false
XMLDocument.loadXML(poster.responseText)
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "33778" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span33778.innerHTML="<STRONG>" & v & "</STRONG>"
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "74427" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span74427.innerHTML="<STRONG>" & v & "</STRONG>"
 window.settimeout "update()",1000 
end sub

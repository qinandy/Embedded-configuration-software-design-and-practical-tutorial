Set poster = CreateObject("Microsoft.XMLHTTP")
Set XMLDocument=CreateObject("Microsoft.XMLDOM")
sub update()
poster.Open "GET", "page1.xml", False
poster.send
XMLDocument.async=false
XMLDocument.loadXML(poster.responseText)
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "59855" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span59855.innerHTML="<STRONG>" & v & "</STRONG>"
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "97718" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span97718.innerHTML="<STRONG>" & v & "</STRONG>"
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "74483" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span74483.innerHTML="<STRONG>" & v & "</STRONG>"
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "2835" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span2835.innerHTML="<STRONG>" & v & "</STRONG>"
 window.settimeout "update()",1000 
end sub

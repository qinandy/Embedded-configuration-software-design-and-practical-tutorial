Set poster = CreateObject("Microsoft.XMLHTTP")
Set XMLDocument=CreateObject("Microsoft.XMLDOM")
sub update()
poster.Open "GET", "page2.xml", False
poster.send
XMLDocument.async=false
XMLDocument.loadXML(poster.responseText)
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "27690" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span27690.innerHTML="<STRONG>" & v & "</STRONG>"
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "6515" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span6515.innerHTML="<STRONG>" & v & "</STRONG>"
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "18937" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span18937.innerHTML="<STRONG>" & v & "</STRONG>"
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "41791" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span41791.innerHTML="<STRONG>" & v & "</STRONG>"
 window.settimeout "update()",1000 
end sub

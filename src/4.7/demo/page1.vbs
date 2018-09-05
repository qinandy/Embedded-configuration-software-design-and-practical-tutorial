Set poster = CreateObject("Microsoft.XMLHTTP")
Set XMLDocument=CreateObject("Microsoft.XMLDOM")
sub update()
poster.Open "GET", "page1.xml", False
poster.send
XMLDocument.async=false
XMLDocument.loadXML(poster.responseText)
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "90060" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span90060.innerHTML="<STRONG>" & v & "</STRONG>"
set node=XMLDocument.documentElement.selectSingleNode("//object[@id=" & chr(34) & "51028" & chr(34) & "]/value")
v= node.firstChild.nodeValue
Span51028.innerHTML="<STRONG>" & v & "</STRONG>"
 window.settimeout "update()",1000 
end sub

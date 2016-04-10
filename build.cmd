if not exist out (md out)
c:\WINDOWS\Microsoft.NET\Framework\v3.5\csc /nologo /debug+ /t:library /define:TRACE /out:out\ReverseProxy.dll code\ReverseProxy.cs

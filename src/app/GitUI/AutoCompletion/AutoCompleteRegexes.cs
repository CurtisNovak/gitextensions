﻿namespace GitUI.AutoCompletion;

internal static class AutoCompleteRegexes
{
    internal const string Default = """
.asm = ^([\w]+):
.au3, .auh = Func\s+([\w]+)|\$([\w]+)
.bas, .frm, .cls = ^\s*(?:(?:Public (?:Default )?|Private )?(?:Sub|Function|Property Get|Property Set|Property Let) ?)(\w+)\(|^Attribute VB_Name = "(\w+)"
.bat = ^\:(\w+)|^set\s+(\w+)
.build = \sname\s*=\s*\"(\w+)\"
.cbl, .cpy = ^.{6} ([A-Z][A-Z0-9-]*)(?: SECTION)?\.
.c, .cc, .cpp, .cxx = \s(?:[\w]+)::([\w]+)|[ \t:.>]([\w]+)\s?\([\w\s,.)]*\);|^#\s*define\s+(\w+)
.asp, .aspx, .cs = (?:(?:(?:public|protected|private|internal)\s+(?:[\w.]+(?:\s*<[<>\w\s.,]+>)?\s+)*([\w.]+)(?:\s*<[<>\w\s.,]+>)?)|(?:namespace\s+([\w.]+))|(?:new\s+([\w]+))|(?:using\s+([\w.]+)))
.h, .hpp, .hxx = ^\s*(?:class|struct)\s+([\w]+)|^\s+(?:\w+\s+)*([\w]+)\s*\(|^#define\s+(\w+)
.html = \"#(\w+)\"
.java, .groovy = (?:public|protected|private|internal)\s+(?:[\w.]+\s+)*([\w.]+)|class\s+([\w]+)(?:(?:\s+)?(?:extends|implements)(?:\s+)?([\w]+)?)
.js = (?:(?:prototype\.|this\.)(\w+)\s*=\s*)?function\s*(?:(\w*)\s*)\(
.pas = (\w+)\s+=\s+(?:class|record|interface)|(?:procedure|function|property|constructor)\s+(\w+)
.php = ^\s*class\s+(\w+)|^\s*(?:(?:public|private)?\s+function)\s+(\w+)|::(\w+)|->(\w+)
.cgi, .pl, .pm = ^\s*sub\s+(\w+)|\s*(?:package|use)\s+([\w\:]+)
.ps1 = ^\s*(?:function)\s+(\w+[\w\-]*)
.py, .pyw, .rb = ^\s*(?:class|def)\s+(\w+)
.sd = ^\s*(?:procedure|function)\s+(\w+)
.vb, .vb6 = (?:class|function|sub)\s+(\w+)(?:\s*(?:[\(\']|$))
.xaml = \sname\s*=\s*\"(\w+)\"
""";
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticProgramAnalyzer.Tests
{
    [TestClass]
    public class Figures
    {
        string _program =
            @"procedure Circle {
t = 1;
a = t + 10;
d = t * a + 2;
call Triangle;
b = t + a;
call Hexagon;
b = t + a;
if t then {
k = a - d;
while c {
d = d + t;
c = d + 1; }
a = d + t; }
else {
a = d + t;
call Hexagon;
c = c - 1; }
call Rectangle; }
procedure Rectangle {
while c {
t = d + 3 * a + c;
call Triangle;
c = c + 20; }
d = t; }
procedure Triangle {
while d {
if t then {
d = t + 2; }
else {
a = t * a + d + k * b; }}
c = t + k + d; }
procedure Hexagon {
t = a + t; }";
    }
}

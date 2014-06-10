// This code was generated by the Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005-2010
// (see accompanying GPPGcopyright.rtf)

// GPPG version 1.5.0
// Machine:  SUNTORY
// DateTime: 05-Jun-14 20:26:30
// UserName: nigelh
// Input file <CbParser.y - 05-Jun-14 19:22:52>

// options: conflicts lines diagnose & report gplex conflicts

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using QUT.Gppg;

namespace FrontEnd
{
public enum Tokens {error=126,
    EOF=127,OROR=128,ANDAND=129,EQEQ=130,NOTEQ=131,GTEQ=132,
    LTEQ=133,Kwd_break=134,Kwd_char=135,Kwd_class=136,Kwd_const=137,Kwd_else=138,
    Kwd_if=139,Kwd_int=140,Kwd_new=141,Kwd_null=142,Kwd_override=143,Kwd_public=144,
    Kwd_return=145,Kwd_static=146,Kwd_string=147,Kwd_using=148,Kwd_virtual=149,Kwd_void=150,
    Kwd_while=151,PLUSPLUS=152,MINUSMINUS=153,Ident=154,CharConst=155,IntConst=156,
    StringConst=157};

// Abstract base class for GPLEX scanners
public abstract class ScanBase : AbstractScanner<AST,LexLocation> {
  private LexLocation __yylloc = new LexLocation();
  public override LexLocation yylloc { get { return __yylloc; } set { __yylloc = value; } }
  protected virtual bool yywrap() { return true; }
}

// Utility class for encapsulating token information
public class ScanObj {
  public int token;
  public AST yylval;
  public LexLocation yylloc;
  public ScanObj( int t, AST val, LexLocation loc ) {
    this.token = t; this.yylval = val; this.yylloc = loc;
  }
}

public class Parser: ShiftReduceParser<AST, LexLocation>
{
#pragma warning disable 649
  private static Dictionary<int, string> aliasses;
#pragma warning restore 649
  private static Rule[] rules = new Rule[96];
  private static State[] states = new State[190];
  private static string[] nonTerms = new string[] {
      "Program", "$accept", "UsingList", "ClassList", "Identifier", "ClassDecl", 
      "DeclList", "ConstDecl", "FieldDecl", "MethodDecl", "Type", "InitVal", 
      "IdentList", "MethodAttr", "MethodType", "OptFormals", "Block", "FormalPars", 
      "FormalDecl", "TypeName", "BuiltInType", "Statement", "Designator", "Expr", 
      "OptActuals", "ActPars", "DeclsAndStmts", "LocalDecl", "UnaryExpr", "UnaryExprNotUMinus", 
      "Qualifiers", };

  static Parser() {
    states[0] = new State(-3,new int[]{-1,1,-3,3});
    states[1] = new State(new int[]{127,2});
    states[2] = new State(-1);
    states[3] = new State(new int[]{148,186,136,6},new int[]{-4,4,-6,189});
    states[4] = new State(new int[]{136,6,127,-2},new int[]{-6,5});
    states[5] = new State(-6);
    states[6] = new State(new int[]{154,24},new int[]{-5,7});
    states[7] = new State(new int[]{123,8,58,181});
    states[8] = new State(-9,new int[]{-7,9});
    states[9] = new State(new int[]{125,10,144,14},new int[]{-8,11,-9,12,-10,13});
    states[10] = new State(-7);
    states[11] = new State(-10);
    states[12] = new State(-11);
    states[13] = new State(-12);
    states[14] = new State(new int[]{137,15,154,24,140,30,147,31,135,32,146,178,149,179,143,180},new int[]{-11,33,-14,39,-20,25,-5,28,-21,29});
    states[15] = new State(new int[]{154,24,140,30,147,31,135,32},new int[]{-11,16,-20,25,-5,28,-21,29});
    states[16] = new State(new int[]{154,24},new int[]{-5,17});
    states[17] = new State(new int[]{61,18});
    states[18] = new State(new int[]{156,21,155,22,157,23},new int[]{-12,19});
    states[19] = new State(new int[]{59,20});
    states[20] = new State(-13);
    states[21] = new State(-14);
    states[22] = new State(-15);
    states[23] = new State(-16);
    states[24] = new State(-95);
    states[25] = new State(new int[]{91,26,154,-31});
    states[26] = new State(new int[]{93,27});
    states[27] = new State(-32);
    states[28] = new State(-33);
    states[29] = new State(-34);
    states[30] = new State(-35);
    states[31] = new State(-36);
    states[32] = new State(-37);
    states[33] = new State(new int[]{154,24},new int[]{-13,34,-5,38});
    states[34] = new State(new int[]{59,35,44,36});
    states[35] = new State(-17);
    states[36] = new State(new int[]{154,24},new int[]{-5,37});
    states[37] = new State(-18);
    states[38] = new State(-19);
    states[39] = new State(new int[]{150,176,154,24,140,30,147,31,135,32},new int[]{-15,40,-11,177,-20,25,-5,28,-21,29});
    states[40] = new State(new int[]{154,24},new int[]{-5,41});
    states[41] = new State(new int[]{40,42});
    states[42] = new State(new int[]{154,24,140,30,147,31,135,32,41,-26},new int[]{-16,43,-18,170,-19,175,-11,173,-20,25,-5,28,-21,29});
    states[43] = new State(new int[]{41,44});
    states[44] = new State(new int[]{123,46},new int[]{-17,45});
    states[45] = new State(-20);
    states[46] = new State(-58,new int[]{-27,47});
    states[47] = new State(new int[]{125,48,154,24,139,142,151,149,134,154,145,156,123,46,59,161,140,30,147,31,135,32},new int[]{-22,49,-28,50,-23,51,-5,137,-17,160,-20,162,-21,165});
    states[48] = new State(-54);
    states[49] = new State(-59);
    states[50] = new State(-60);
    states[51] = new State(new int[]{61,52,40,129,152,133,153,135});
    states[52] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,53,-29,81,-30,84,-23,85,-5,92});
    states[53] = new State(new int[]{59,54,128,55,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79});
    states[54] = new State(-38);
    states[55] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,56,-29,81,-30,84,-23,85,-5,92});
    states[56] = new State(new int[]{128,-61,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,59,-61,44,-61,41,-61,93,-61});
    states[57] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,58,-29,81,-30,84,-23,85,-5,92});
    states[58] = new State(new int[]{128,-62,129,-62,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,59,-62,44,-62,41,-62,93,-62});
    states[59] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,60,-29,81,-30,84,-23,85,-5,92});
    states[60] = new State(new int[]{128,-63,129,-63,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,59,-63,44,-63,41,-63,93,-63});
    states[61] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,62,-29,81,-30,84,-23,85,-5,92});
    states[62] = new State(new int[]{128,-64,129,-64,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,59,-64,44,-64,41,-64,93,-64});
    states[63] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,64,-29,81,-30,84,-23,85,-5,92});
    states[64] = new State(new int[]{128,-65,129,-65,130,-65,131,-65,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,59,-65,44,-65,41,-65,93,-65});
    states[65] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,66,-29,81,-30,84,-23,85,-5,92});
    states[66] = new State(new int[]{128,-66,129,-66,130,-66,131,-66,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,59,-66,44,-66,41,-66,93,-66});
    states[67] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,68,-29,81,-30,84,-23,85,-5,92});
    states[68] = new State(new int[]{128,-67,129,-67,130,-67,131,-67,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,59,-67,44,-67,41,-67,93,-67});
    states[69] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,70,-29,81,-30,84,-23,85,-5,92});
    states[70] = new State(new int[]{128,-68,129,-68,130,-68,131,-68,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,59,-68,44,-68,41,-68,93,-68});
    states[71] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,72,-29,81,-30,84,-23,85,-5,92});
    states[72] = new State(new int[]{128,-69,129,-69,130,-69,131,-69,133,-69,60,-69,132,-69,62,-69,43,-69,45,-69,42,75,47,77,37,79,59,-69,44,-69,41,-69,93,-69});
    states[73] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,74,-29,81,-30,84,-23,85,-5,92});
    states[74] = new State(new int[]{128,-70,129,-70,130,-70,131,-70,133,-70,60,-70,132,-70,62,-70,43,-70,45,-70,42,75,47,77,37,79,59,-70,44,-70,41,-70,93,-70});
    states[75] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,76,-29,81,-30,84,-23,85,-5,92});
    states[76] = new State(-71);
    states[77] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,78,-29,81,-30,84,-23,85,-5,92});
    states[78] = new State(-72);
    states[79] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,80,-29,81,-30,84,-23,85,-5,92});
    states[80] = new State(-73);
    states[81] = new State(-74);
    states[82] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,83,-29,81,-30,84,-23,85,-5,92});
    states[83] = new State(new int[]{128,-75,129,-75,130,-75,131,-75,133,-75,60,-75,132,-75,62,-75,43,-75,45,-75,42,75,47,77,37,79,59,-75,44,-75,41,-75,93,-75});
    states[84] = new State(-76);
    states[85] = new State(new int[]{40,86,59,-77,128,-77,129,-77,130,-77,131,-77,133,-77,60,-77,132,-77,62,-77,43,-77,45,-77,42,-77,47,-77,37,-77,44,-77,41,-77,93,-77});
    states[86] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117,41,-50},new int[]{-25,87,-26,89,-24,128,-29,81,-30,84,-23,85,-5,92});
    states[87] = new State(new int[]{41,88});
    states[88] = new State(-78);
    states[89] = new State(new int[]{44,90,41,-51});
    states[90] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,91,-29,81,-30,84,-23,85,-5,92});
    states[91] = new State(new int[]{128,55,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,44,-52,41,-52});
    states[92] = new State(new int[]{46,94,91,97,40,-94,59,-94,128,-94,129,-94,130,-94,131,-94,133,-94,60,-94,132,-94,62,-94,43,-94,45,-94,42,-94,47,-94,37,-94,44,-94,41,-94,93,-94,61,-94,152,-94,153,-94},new int[]{-31,93});
    states[93] = new State(-90);
    states[94] = new State(new int[]{154,24},new int[]{-5,95});
    states[95] = new State(new int[]{46,94,91,97,40,-94,59,-94,128,-94,129,-94,130,-94,131,-94,133,-94,60,-94,132,-94,62,-94,43,-94,45,-94,42,-94,47,-94,37,-94,44,-94,41,-94,93,-94,61,-94,152,-94,153,-94},new int[]{-31,96});
    states[96] = new State(-91);
    states[97] = new State(new int[]{93,101,45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,98,-29,81,-30,84,-23,85,-5,92});
    states[98] = new State(new int[]{93,99,128,55,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79});
    states[99] = new State(new int[]{46,94,91,97,40,-94,59,-94,128,-94,129,-94,130,-94,131,-94,133,-94,60,-94,132,-94,62,-94,43,-94,45,-94,42,-94,47,-94,37,-94,44,-94,41,-94,93,-94,61,-94,152,-94,153,-94},new int[]{-31,100});
    states[100] = new State(-92);
    states[101] = new State(new int[]{46,94,91,97,40,-94,59,-94,128,-94,129,-94,130,-94,131,-94,133,-94,60,-94,132,-94,62,-94,43,-94,45,-94,42,-94,47,-94,37,-94,44,-94,41,-94,93,-94,61,-94,152,-94,153,-94},new int[]{-31,102});
    states[102] = new State(-93);
    states[103] = new State(-79);
    states[104] = new State(-80);
    states[105] = new State(-81);
    states[106] = new State(new int[]{46,107,59,-82,128,-82,129,-82,130,-82,131,-82,133,-82,60,-82,132,-82,62,-82,43,-82,45,-82,42,-82,47,-82,37,-82,44,-82,41,-82,93,-82});
    states[107] = new State(new int[]{154,24},new int[]{-5,108});
    states[108] = new State(-83);
    states[109] = new State(new int[]{154,24,140,30,147,31,135,32},new int[]{-5,110,-20,113,-21,29});
    states[110] = new State(new int[]{40,111,91,-33});
    states[111] = new State(new int[]{41,112});
    states[112] = new State(-84);
    states[113] = new State(new int[]{91,114});
    states[114] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,115,-29,81,-30,84,-23,85,-5,92});
    states[115] = new State(new int[]{93,116,128,55,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79});
    states[116] = new State(-85);
    states[117] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117,140,30,147,31,135,32},new int[]{-24,118,-21,121,-29,81,-30,84,-23,85,-5,92});
    states[118] = new State(new int[]{41,119,128,55,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79});
    states[119] = new State(new int[]{154,24,142,103,156,104,155,105,157,106,141,109,40,117,59,-86,128,-86,129,-86,130,-86,131,-86,133,-86,60,-86,132,-86,62,-86,43,-86,45,-86,42,-86,47,-86,37,-86,44,-86,41,-86,93,-86},new int[]{-30,120,-23,85,-5,92});
    states[120] = new State(-87);
    states[121] = new State(new int[]{41,122,91,124});
    states[122] = new State(new int[]{154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-30,123,-23,85,-5,92});
    states[123] = new State(-88);
    states[124] = new State(new int[]{93,125});
    states[125] = new State(new int[]{41,126});
    states[126] = new State(new int[]{154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-30,127,-23,85,-5,92});
    states[127] = new State(-89);
    states[128] = new State(new int[]{128,55,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79,44,-53,41,-53});
    states[129] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117,41,-50},new int[]{-25,130,-26,89,-24,128,-29,81,-30,84,-23,85,-5,92});
    states[130] = new State(new int[]{41,131});
    states[131] = new State(new int[]{59,132});
    states[132] = new State(-39);
    states[133] = new State(new int[]{59,134});
    states[134] = new State(-40);
    states[135] = new State(new int[]{59,136});
    states[136] = new State(-41);
    states[137] = new State(new int[]{91,138,46,94,154,-33,61,-94,40,-94,152,-94,153,-94},new int[]{-31,93});
    states[138] = new State(new int[]{93,139,45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,98,-29,81,-30,84,-23,85,-5,92});
    states[139] = new State(new int[]{154,24,46,94,91,97,61,-94,40,-94,152,-94,153,-94},new int[]{-13,140,-31,102,-5,38});
    states[140] = new State(new int[]{59,141,44,36});
    states[141] = new State(-56);
    states[142] = new State(new int[]{40,143});
    states[143] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,144,-29,81,-30,84,-23,85,-5,92});
    states[144] = new State(new int[]{41,145,128,55,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79});
    states[145] = new State(new int[]{154,24,139,142,151,149,134,154,145,156,123,46,59,161},new int[]{-22,146,-23,51,-5,92,-17,160});
    states[146] = new State(new int[]{138,147,125,-43,154,-43,139,-43,151,-43,134,-43,145,-43,123,-43,59,-43,140,-43,147,-43,135,-43});
    states[147] = new State(new int[]{154,24,139,142,151,149,134,154,145,156,123,46,59,161},new int[]{-22,148,-23,51,-5,92,-17,160});
    states[148] = new State(-42);
    states[149] = new State(new int[]{40,150});
    states[150] = new State(new int[]{45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,151,-29,81,-30,84,-23,85,-5,92});
    states[151] = new State(new int[]{41,152,128,55,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79});
    states[152] = new State(new int[]{154,24,139,142,151,149,134,154,145,156,123,46,59,161},new int[]{-22,153,-23,51,-5,92,-17,160});
    states[153] = new State(-44);
    states[154] = new State(new int[]{59,155});
    states[155] = new State(-45);
    states[156] = new State(new int[]{59,157,45,82,154,24,142,103,156,104,155,105,157,106,141,109,40,117},new int[]{-24,158,-29,81,-30,84,-23,85,-5,92});
    states[157] = new State(-46);
    states[158] = new State(new int[]{59,159,128,55,129,57,130,59,131,61,133,63,60,65,132,67,62,69,43,71,45,73,42,75,47,77,37,79});
    states[159] = new State(-47);
    states[160] = new State(-48);
    states[161] = new State(-49);
    states[162] = new State(new int[]{154,24},new int[]{-13,163,-5,38});
    states[163] = new State(new int[]{59,164,44,36});
    states[164] = new State(-55);
    states[165] = new State(new int[]{91,166,154,-34});
    states[166] = new State(new int[]{93,167});
    states[167] = new State(new int[]{154,24},new int[]{-13,168,-5,38});
    states[168] = new State(new int[]{59,169,44,36});
    states[169] = new State(-57);
    states[170] = new State(new int[]{44,171,41,-27});
    states[171] = new State(new int[]{154,24,140,30,147,31,135,32},new int[]{-19,172,-11,173,-20,25,-5,28,-21,29});
    states[172] = new State(-29);
    states[173] = new State(new int[]{154,24},new int[]{-5,174});
    states[174] = new State(-30);
    states[175] = new State(-28);
    states[176] = new State(-24);
    states[177] = new State(-25);
    states[178] = new State(-21);
    states[179] = new State(-22);
    states[180] = new State(-23);
    states[181] = new State(new int[]{154,24},new int[]{-5,182});
    states[182] = new State(new int[]{123,183});
    states[183] = new State(-9,new int[]{-7,184});
    states[184] = new State(new int[]{125,185,144,14},new int[]{-8,11,-9,12,-10,13});
    states[185] = new State(-8);
    states[186] = new State(new int[]{154,24},new int[]{-5,187});
    states[187] = new State(new int[]{59,188});
    states[188] = new State(-4);
    states[189] = new State(-5);

    for (int sNo = 0; sNo < states.Length; sNo++) states[sNo].number = sNo;

    rules[1] = new Rule(-2, new int[]{-1,127});
    rules[2] = new Rule(-1, new int[]{-3,-4});
    rules[3] = new Rule(-3, new int[]{});
    rules[4] = new Rule(-3, new int[]{-3,148,-5,59});
    rules[5] = new Rule(-4, new int[]{-6});
    rules[6] = new Rule(-4, new int[]{-4,-6});
    rules[7] = new Rule(-6, new int[]{136,-5,123,-7,125});
    rules[8] = new Rule(-6, new int[]{136,-5,58,-5,123,-7,125});
    rules[9] = new Rule(-7, new int[]{});
    rules[10] = new Rule(-7, new int[]{-7,-8});
    rules[11] = new Rule(-7, new int[]{-7,-9});
    rules[12] = new Rule(-7, new int[]{-7,-10});
    rules[13] = new Rule(-8, new int[]{144,137,-11,-5,61,-12,59});
    rules[14] = new Rule(-12, new int[]{156});
    rules[15] = new Rule(-12, new int[]{155});
    rules[16] = new Rule(-12, new int[]{157});
    rules[17] = new Rule(-9, new int[]{144,-11,-13,59});
    rules[18] = new Rule(-13, new int[]{-13,44,-5});
    rules[19] = new Rule(-13, new int[]{-5});
    rules[20] = new Rule(-10, new int[]{144,-14,-15,-5,40,-16,41,-17});
    rules[21] = new Rule(-14, new int[]{146});
    rules[22] = new Rule(-14, new int[]{149});
    rules[23] = new Rule(-14, new int[]{143});
    rules[24] = new Rule(-15, new int[]{150});
    rules[25] = new Rule(-15, new int[]{-11});
    rules[26] = new Rule(-16, new int[]{});
    rules[27] = new Rule(-16, new int[]{-18});
    rules[28] = new Rule(-18, new int[]{-19});
    rules[29] = new Rule(-18, new int[]{-18,44,-19});
    rules[30] = new Rule(-19, new int[]{-11,-5});
    rules[31] = new Rule(-11, new int[]{-20});
    rules[32] = new Rule(-11, new int[]{-20,91,93});
    rules[33] = new Rule(-20, new int[]{-5});
    rules[34] = new Rule(-20, new int[]{-21});
    rules[35] = new Rule(-21, new int[]{140});
    rules[36] = new Rule(-21, new int[]{147});
    rules[37] = new Rule(-21, new int[]{135});
    rules[38] = new Rule(-22, new int[]{-23,61,-24,59});
    rules[39] = new Rule(-22, new int[]{-23,40,-25,41,59});
    rules[40] = new Rule(-22, new int[]{-23,152,59});
    rules[41] = new Rule(-22, new int[]{-23,153,59});
    rules[42] = new Rule(-22, new int[]{139,40,-24,41,-22,138,-22});
    rules[43] = new Rule(-22, new int[]{139,40,-24,41,-22});
    rules[44] = new Rule(-22, new int[]{151,40,-24,41,-22});
    rules[45] = new Rule(-22, new int[]{134,59});
    rules[46] = new Rule(-22, new int[]{145,59});
    rules[47] = new Rule(-22, new int[]{145,-24,59});
    rules[48] = new Rule(-22, new int[]{-17});
    rules[49] = new Rule(-22, new int[]{59});
    rules[50] = new Rule(-25, new int[]{});
    rules[51] = new Rule(-25, new int[]{-26});
    rules[52] = new Rule(-26, new int[]{-26,44,-24});
    rules[53] = new Rule(-26, new int[]{-24});
    rules[54] = new Rule(-17, new int[]{123,-27,125});
    rules[55] = new Rule(-28, new int[]{-20,-13,59});
    rules[56] = new Rule(-28, new int[]{-5,91,93,-13,59});
    rules[57] = new Rule(-28, new int[]{-21,91,93,-13,59});
    rules[58] = new Rule(-27, new int[]{});
    rules[59] = new Rule(-27, new int[]{-27,-22});
    rules[60] = new Rule(-27, new int[]{-27,-28});
    rules[61] = new Rule(-24, new int[]{-24,128,-24});
    rules[62] = new Rule(-24, new int[]{-24,129,-24});
    rules[63] = new Rule(-24, new int[]{-24,130,-24});
    rules[64] = new Rule(-24, new int[]{-24,131,-24});
    rules[65] = new Rule(-24, new int[]{-24,133,-24});
    rules[66] = new Rule(-24, new int[]{-24,60,-24});
    rules[67] = new Rule(-24, new int[]{-24,132,-24});
    rules[68] = new Rule(-24, new int[]{-24,62,-24});
    rules[69] = new Rule(-24, new int[]{-24,43,-24});
    rules[70] = new Rule(-24, new int[]{-24,45,-24});
    rules[71] = new Rule(-24, new int[]{-24,42,-24});
    rules[72] = new Rule(-24, new int[]{-24,47,-24});
    rules[73] = new Rule(-24, new int[]{-24,37,-24});
    rules[74] = new Rule(-24, new int[]{-29});
    rules[75] = new Rule(-29, new int[]{45,-24});
    rules[76] = new Rule(-29, new int[]{-30});
    rules[77] = new Rule(-30, new int[]{-23});
    rules[78] = new Rule(-30, new int[]{-23,40,-25,41});
    rules[79] = new Rule(-30, new int[]{142});
    rules[80] = new Rule(-30, new int[]{156});
    rules[81] = new Rule(-30, new int[]{155});
    rules[82] = new Rule(-30, new int[]{157});
    rules[83] = new Rule(-30, new int[]{157,46,-5});
    rules[84] = new Rule(-30, new int[]{141,-5,40,41});
    rules[85] = new Rule(-30, new int[]{141,-20,91,-24,93});
    rules[86] = new Rule(-30, new int[]{40,-24,41});
    rules[87] = new Rule(-30, new int[]{40,-24,41,-30});
    rules[88] = new Rule(-30, new int[]{40,-21,41,-30});
    rules[89] = new Rule(-30, new int[]{40,-21,91,93,41,-30});
    rules[90] = new Rule(-23, new int[]{-5,-31});
    rules[91] = new Rule(-31, new int[]{46,-5,-31});
    rules[92] = new Rule(-31, new int[]{91,-24,93,-31});
    rules[93] = new Rule(-31, new int[]{91,93,-31});
    rules[94] = new Rule(-31, new int[]{});
    rules[95] = new Rule(-5, new int[]{154});
  }

  protected override void Initialize() {
    this.InitSpecialTokens((int)Tokens.error, (int)Tokens.EOF);
    this.InitStates(states);
    this.InitRules(rules);
    this.InitNonTerminals(nonTerms);
  }

  protected override void DoAction(int action)
  {
#pragma warning disable 162, 1522
    switch (action)
    {
      case 2: // Program -> UsingList, ClassList
#line 59 "CbParser.y"
{ Tree = AST.NonLeaf(NodeType.Program, ValueStack[ValueStack.Depth-2].LineNumber, ValueStack[ValueStack.Depth-2], ValueStack[ValueStack.Depth-1]); }
        break;
      case 3: // UsingList -> /* empty */
#line 63 "CbParser.y"
{ CurrentSemanticValue = AST.Kary(NodeType.UsingList, LineNumber); }
        break;
      case 4: // UsingList -> UsingList, Kwd_using, Identifier, ';'
#line 65 "CbParser.y"
{ ValueStack[ValueStack.Depth-4].AddChild(ValueStack[ValueStack.Depth-2]);  CurrentSemanticValue = ValueStack[ValueStack.Depth-4]; }
        break;
      case 5: // ClassList -> ClassDecl
#line 69 "CbParser.y"
{ CurrentSemanticValue = AST.Kary(NodeType.ClassList, LineNumber); }
        break;
      case 6: // ClassList -> ClassList, ClassDecl
#line 71 "CbParser.y"
{ ValueStack[ValueStack.Depth-2].AddChild(ValueStack[ValueStack.Depth-1]);  CurrentSemanticValue = ValueStack[ValueStack.Depth-2]; }
        break;
      case 7: // ClassDecl -> Kwd_class, Identifier, '{', DeclList, '}'
#line 75 "CbParser.y"
{ CurrentSemanticValue = AST.NonLeaf(NodeType.Class, ValueStack[ValueStack.Depth-4].LineNumber, ValueStack[ValueStack.Depth-4], null, ValueStack[ValueStack.Depth-2]); }
        break;
      case 8: // ClassDecl -> Kwd_class, Identifier, ':', Identifier, '{', DeclList, '}'
#line 77 "CbParser.y"
{ CurrentSemanticValue = AST.NonLeaf(NodeType.Class, ValueStack[ValueStack.Depth-6].LineNumber, ValueStack[ValueStack.Depth-6], ValueStack[ValueStack.Depth-4], ValueStack[ValueStack.Depth-2]); }
        break;
      case 95: // Identifier -> Ident
#line 218 "CbParser.y"
{ CurrentSemanticValue = AST.Leaf(NodeType.Ident, LineNumber, lexer.yytext); }
        break;
    }
#pragma warning restore 162, 1522
  }

  protected override string TerminalToString(int terminal)
  {
    if (aliasses != null && aliasses.ContainsKey(terminal))
        return aliasses[terminal];
    else if (((Tokens)terminal).ToString() != terminal.ToString(CultureInfo.InvariantCulture))
        return ((Tokens)terminal).ToString();
    else
        return CharToString((char)terminal);
  }

#line 221 "CbParser.y"

#line 222 "CbParser.y"
// returns the AST constructed for the Cb program
#line 223 "CbParser.y"
public AST Tree { get; private set; }
#line 224 "CbParser.y"

#line 225 "CbParser.y"
private Scanner lexer;
#line 226 "CbParser.y"

#line 227 "CbParser.y"
// returns the lexer's current line number
#line 228 "CbParser.y"
public int LineNumber {
#line 229 "CbParser.y"
    get{ return lexer.LineNumber == 0? 1 : lexer.LineNumber; }
#line 230 "CbParser.y"
}
#line 231 "CbParser.y"

#line 232 "CbParser.y"
// Use this function for reporting non-fatal errors discovered
#line 233 "CbParser.y"
// while parsing and building the AST.
#line 234 "CbParser.y"
// An example usage is:
#line 235 "CbParser.y"
//    yyerror( "Identifier {0} has not been declared", idname );
#line 236 "CbParser.y"
public void yyerror( string format, params Object[] args ) {
#line 237 "CbParser.y"
    Console.Write("{0}: ", LineNumber);
#line 238 "CbParser.y"
    Console.WriteLine(format, args);
#line 239 "CbParser.y"
}
#line 240 "CbParser.y"

#line 241 "CbParser.y"
// The parser needs a suitable constructor
#line 242 "CbParser.y"
public Parser( Scanner src ) : base(null) {
#line 243 "CbParser.y"
    lexer = src;
#line 244 "CbParser.y"
    Scanner = src;
#line 245 "CbParser.y"
}
#line 246 "CbParser.y"

#line 247 "CbParser.y"

}
}

using System;

class Test{
	public override void test(){

	}
	//this is a single line comment
	public virtual int getZero(){
		return 0;
	}
	/*This is a
	multiple /*
	line incorrectly nested
	comment */

	public static void Main(String[] args){

		int x = 2;
		int y = x;
		//qwerty
		if(x==2){
			int y = 3;
		}
	}
}

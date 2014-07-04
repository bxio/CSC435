// test of everything, but without a using clause

class Foo {
    public const int theAnswer = 42;
    public const string hiThere = "hello";
    public const char itsAnX = 'x';

    public int a;
    public string b;
    public char c;

    public static void Main() {
        Foo f;
        f = new Bar();
        int r;
        r = f.Umm(3,4);
        
        int [] qwe;
        qwe = new int[5];
        qwe[4] = r;
        qwe[3] = 3;
    }

    public virtual int Ummm( int a, int b ) {
        System.Console.WriteLine("This is Foo");
        return a+b;
    }
    
}

class Idiot {

    public static int multiply(int x, int y, int z){
        return x*y*z;
    }

}


class Bar : Foo {
    public int x;

    public override int Umm( int aa, int bb ) {
        //System.Console.WriteLine("This is Bar");
        return a-b;
    }
}

public class Math{
    public enum Operations{
        Add,
        Subtract,
        Multiply,
        Divide,
	Power,
	Sqrt
    }

    public static double Calculate(double num1, double num2, Operations operation){
        switch(operation){
            case Operations.Add:
                return num1 + num2;
            case Operations.Subtract:
                return num1 - num2;
            case Operations.Multiply:
                return num1 * num2;
            case Operations.Divide:
                return num1 / num2;
            case Operations.Power:
                return num1 * num1;
        case Operations.Sqrt:
                return System.Math.Abs(num1);
            default:
                return 0;
        }
    }
    
}

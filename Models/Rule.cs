namespace CustomPress.Models;

record Rule(Func<double, double> Transform, string Description);

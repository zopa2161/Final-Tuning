public interface IInputProvider
{
    // [0]: Steering, [1]: Throttle, [2]: Brake
    float[] GetInput(); 
}
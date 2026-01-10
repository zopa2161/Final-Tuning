using UnityEngine;

public class InputManager
{
        private PhysicalInput _steeringInput;
        private PhysicalInput _throttleInput;
        private PhysicalInput _brakeInput;
        
        private CarMainSystem _mainSystem;

        public void setup(CarMainSystem mainSystem)
        {
                _steeringInput = new PhysicalInput(10, 10);
                _throttleInput = new PhysicalInput(5, 10);
                _brakeInput = new PhysicalInput(5, 10);
                _mainSystem = mainSystem;
        }
        

        public void Step(float[] inputs)
        {       
                //Debug.Log("inputs "+inputs[0]+" "+inputs[1]+" "+inputs[2]);
                var context = _mainSystem.GetCurrentContext();
                context.SteeringAngle = _steeringInput.tick(context.SteeringAngle, inputs[0], context.deltaTime);
                context.Throttle = _throttleInput.tick(context.Throttle, Mathf.Clamp01(inputs[1]), context.deltaTime);
                context.Brake = _brakeInput.tick(context.Brake, Mathf.Clamp01(inputs[2]), context.deltaTime);
                //Debug.Log("Steering Angle: " + context.SteeringAngle + " Throttle: " + context.Throttle + " Brake: " + context.Brake + "");

        }
        
}
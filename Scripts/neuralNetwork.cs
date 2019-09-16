using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class neuralNetwork : MonoBehaviour
{

    int[] numberOfNeuronsInLayers;
    float[][][] weightMatrix;
    float[][] neuronValuesMatrix;
    GameObject movementController;
    movement movementScript;
    geneticAlgoritm geneticScript;
    private Rigidbody rb;
    bool alive = true;
    bool[] passedcorners = new bool[12];
    int howManyTriggersPassed = 0;


    //function to create and manage the 'sensors' attached to the players so that they can 'see' the course
    //this provides most of the input to the neural nets
    //The 5 sensors are each 10f long. If they're not touching anything, the neural net gets an input of '0' for distance and angle
    //If they do hit a wall, then the neural net gets an input of the distance from the player to the wall 
    //as well as an input of the angle the ray hits the wall at 
    void Sensors()
    {
        RaycastHit hit;

        //West sensor 
        if (Physics.Raycast(transform.position, Vector3.left, out hit, 10f))
        {
            neuronValuesMatrix[0][2] = Vector3.Distance(transform.position, hit.point);
            neuronValuesMatrix[0][3] = Vector3.Angle(hit.point - transform.position, transform.forward);

        }

        else
        {
            neuronValuesMatrix[0][2] = 0f;
            neuronValuesMatrix[0][3] = 0f;
        }


        //45 degrees North-West
        if (Physics.Raycast(transform.position, (Vector3.left + Vector3.forward) / 2, out hit, 10f))
        {
            neuronValuesMatrix[0][4] = Vector3.Distance(transform.position, hit.point);
            neuronValuesMatrix[0][5] = Vector3.Angle(hit.point - transform.position, transform.forward);
        }

        else
        {
            neuronValuesMatrix[0][4] = 0f;
            neuronValuesMatrix[0][5] = 0f;
        }

        //North sensor
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, 10f))
        {
            neuronValuesMatrix[0][6] = Vector3.Distance(transform.position, hit.point);
            neuronValuesMatrix[0][7] = Vector3.Angle(hit.point - transform.position, transform.forward);
        }

        else
        {
            neuronValuesMatrix[0][6] = 0f;
            neuronValuesMatrix[0][7] = 0f;
        }

        //North East sensor
        if (Physics.Raycast(transform.position, (Vector3.right + Vector3.forward) / 2, out hit, 10f))
        {
            neuronValuesMatrix[0][8] = Vector3.Distance(transform.position, hit.point);
            neuronValuesMatrix[0][9] = Vector3.Angle(hit.point - transform.position, transform.forward);
        }

        else
        {
            neuronValuesMatrix[0][8] = 0f;
            neuronValuesMatrix[0][9] = 0f;
        }

        //East sensor
        if (Physics.Raycast(transform.position, Vector3.right, out hit, 10f))
        {
            neuronValuesMatrix[0][10] = Vector3.Distance(transform.position, hit.point);
            neuronValuesMatrix[0][11] = Vector3.Angle(hit.point - transform.position, transform.forward);
        }

        else
        {
            neuronValuesMatrix[0][10] = 0f;
            neuronValuesMatrix[0][11] = 0f;
        }

    }


    //create empty neuron matrix when player is created 
    void CreateEmptyNeuronvalueMatrix() {
        neuronValuesMatrix = new float[][]
        {
            new float[]{0,0,0,0,0,0,0,0,0,0,0,0},
            new float[]{0,0,0,0,0,0,0,0},
            new float[]{0,0,0,0}
        };
    }
   

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        movementController = GameObject.FindGameObjectWithTag("GameController");
        movementScript = movementController.GetComponent<movement>();
        geneticScript = movementController.GetComponent<geneticAlgoritm>();
        numberOfNeuronsInLayers = new int[] { 12, 8, 4 };
        CreateEmptyNeuronvalueMatrix();

        for (var i = 0; i < passedcorners.Length; i++)
        {
            passedcorners[i] = false;
        }

    }  


    //this function is called by the genetic algorithm script when creating child players
    //it allows the child player to inherit the weight matrix from parents/with crossover and mutation
    public void inheritWeightMatrix(float[][][] weightMatrixInput)
    {
        weightMatrix = weightMatrixInput;
    }


    //the neural net that takes inputs from the players and then decides what action the player should take next
    private void decideAction()
    {
        //Inputs: the x and z velocity (the course has a smooth ground so y velocity shouldn't be important as they're not going up or down)
        //and the distances and angles that the sensors hit any walls at 
        neuronValuesMatrix[0][0] = rb.velocity.x;
        neuronValuesMatrix[0][1] = rb.velocity.z;
        Sensors();

        //for every layer (omitting the input layer)
        for (var i = 1; i < 3; i++)
        {
            //for every neuron in this layer
            for (var j = 0; j < numberOfNeuronsInLayers[i]; j++)
            {
                float individualNeuronValue = 0.01f; //bias value

                //for each of the neurons in the last layer
                for (var k = 0; k < numberOfNeuronsInLayers[i - 1]; k++)
                {
                    //multiply the neuron value in that last layer by its weight in the weight matrix, then add them all up to get this current neuron value 
                    individualNeuronValue += neuronValuesMatrix[i - 1][k] * weightMatrix[i - 1][j][k];
                }

                //tanh activation function then update neuron value matrix
                if (i == 1)
                {
                    neuronValuesMatrix[i][j] = (float)System.Math.Tanh(individualNeuronValue);
                }
                else if (i == 2)
                {
                    //sigmoid activation to get probability for neurons in last layer
                    neuronValuesMatrix[i][j] = 1 / (1 + Mathf.Exp(-individualNeuronValue));
                }
            }

        }

        //if the value of the first neuron in the last layer is above 0.5, propel player forward
        if (neuronValuesMatrix[2][0] > 0.5)
        {
            rb.AddForce(Vector3.forward * 10);
        }
        //if the value of the second neuron in the last layer is above 0.5, brake
        if (neuronValuesMatrix[2][1] > 0.5)
        {
            rb.AddForce(Vector3.forward * -10);
        }
        //if the value of the third neuron in the last layer is above 0.5, propel player left
        if (neuronValuesMatrix[2][2] > 0.5)
        {
            rb.AddForce(Vector3.left * 10);
        }
        //if the value of the fourth neuron in the last layer is above 0.5, propel player right
        if (neuronValuesMatrix[2][3] > 0.5)
        {
            rb.AddForce(Vector3.right * 10);
        }
    }


    //function used to return weights matrix. Used in the genetic algorithm script to extract current generation's weight matrix to pass onto children 
    public float[][][] GetWeightsMatrix()
    {
        return weightMatrix;
    }


    //if the players pass through set triggers on the course, they get an increased score which ranks their fitness higher
    //these triggers are needed alonside ranking them by who moved the furthest away in the z-direction because at some points in the course, 
    //the players need to move 'horizontally' left or right to go further, but this progress isn't captured by simply using the z-position
    private void OnTriggerEnter(Collider other)
    {
                for (var i=1; i < passedcorners.Length+1; i++){
                string cornername = "corner" + i;
      
                if (other.gameObject.name == cornername && !passedcorners[i-1])
                {
                    howManyTriggersPassed++;
                    passedcorners[i - 1] = true;
                }

            }
    }


    //function used in genetic algorithm script to rank the players' fitness
    public float getCornersPassedScore()
    {
        return howManyTriggersPassed;
    }


    //use fixed update not regular update because using physics functions
    private void FixedUpdate()
    {
  
        //make sure that players aren't allowed to go backwards. They can brake, but not actively go backwards
        if (rb.velocity.z < 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0);
        }
        //if the player's still alive, run the neural net function to decide what action to take next
        if (alive)
        {
            decideAction();
        }
        //if the player is dead, stay where they are and turn red so that a person watching can identify them as dead
        else
        {
            rb.velocity = new Vector3(0, 0, 0);
            GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
            
    }


    //if a player crashes into a wall, they die 
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "wall")
        {
            alive = false;
        }


    }

    //function used in genetic algorithms script to help determine fitness of players
    public bool IsAlive()
    {
        return alive;
    }

}



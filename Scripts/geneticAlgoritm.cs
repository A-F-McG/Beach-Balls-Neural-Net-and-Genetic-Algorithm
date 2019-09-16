using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class geneticAlgoritm : MonoBehaviour
{
    public GameObject playerPrefab;
    List<GameObject> population = new List<GameObject>();
    List<GameObject> furthestPopulation = new List<GameObject>();
    int numberDead = 0;
    float time = 0;
    bool done = false;
    float[][][] bestLittleMatrix;

    //change this value to have more or less 'children' in a generation 
    int populationSize = 150;


    void Start()
    {
        InstantiateInitialPopulation();
        ignoreOtherPlayers();
    }

    // Use fixedupdate and not regular update because of the physics applied to the gameobjects
    void FixedUpdate()
    {
        time += 1 * Time.deltaTime;
        numberDead = 0;

        //order the current population by the ones who've made the most progress on the course to the ones who travelled the least
        furthestPopulation = population.OrderByDescending(go => go.GetComponent<neuralNetwork>().getCornersPassedScore())
        .ThenByDescending(go => go.transform.position.z)
        .ToList();

        //take the top half of that population 
        int quarter = Mathf.RoundToInt(furthestPopulation.Count * 0.5f);
       
        //count the number of players in the top half who have died (crashed in to a wall)
        for (var i=0; i < quarter; i++)
        {
           
            if (furthestPopulation[i].GetComponent<neuralNetwork>().IsAlive().Equals(false))
            {
                numberDead++;
            }
        }


        //if the furthest 5 players are all dead AND the furthest half of the population is dead (with a wiggle room of 3 players still being alive)
        //OR it's been more than a minute
        //then spawn a new population
        if ((furthestPopulation[0].GetComponent<neuralNetwork>().IsAlive().Equals(false) &&
            furthestPopulation[1].GetComponent<neuralNetwork>().IsAlive().Equals(false) &&
            furthestPopulation[2].GetComponent<neuralNetwork>().IsAlive().Equals(false) &&
            furthestPopulation[3].GetComponent<neuralNetwork>().IsAlive().Equals(false) &&
            furthestPopulation[4].GetComponent<neuralNetwork>().IsAlive().Equals(false) &&
             numberDead >= (furthestPopulation.Count * 0.5f)-3)
            ||

            time > 60f)
        {
            NewPopulation();
            ignoreOtherPlayers();
            time = 0;
            furthestPopulation.Clear();
            numberDead = 0;
        }


        //press 'E' at any point to stop creating new generations and instead pick the best performing 'child' and see them traverse the course alone
        if (Input.GetKeyDown(KeyCode.E))
        {
            BestDude();
            done = true;
            ignoreOtherPlayers();
        }


        //press 'X' at any point to manually spawn a new population 
        //generally only needed at the beginning when the players haven't learned to drive forward yet and you don't want to have to wait
        //60 seconds for the next generation 
        if (Input.GetKeyDown(KeyCode.X))
        {
            NewPopulation();
            ignoreOtherPlayers();
            time = 0;
            furthestPopulation.Clear();
            numberDead = 0;
        }

        furthestPopulation.Clear();
    }


    //For the first generation, create a weight matrix with random values between -1 and 1
    public float[][][] CreateWeightMatrix()
    {
        //create empty matrix. 2 rows, 1st row 8 x 12-arrays, 2nd row 4x7 arrays
        float[][][] weightMatrix = new float[][][]
        {
            new float[][]{new float[]{Random.Range(-1f,1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[]{Random.Range(-1f,1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }},
            new float[][]{new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) }, new float[] { Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) } }
        };
        return weightMatrix;
    }


    //instantiate the initial population 
    void InstantiateInitialPopulation()
    {
        for (var i=0; i<populationSize; i++)
        {

            GameObject go = Instantiate(playerPrefab);
            go.GetComponent<neuralNetwork>().inheritWeightMatrix(CreateWeightMatrix());
            population.Add(go);
        }
    }


    //Genetic algorithm - cross over and mutate parents\ weight matrices to create the weight matrix for a new 'child' player
    float[][][] CrossOverAndMutate(float[][][] parent1, float[][][] parent2)
    {
        //set child weight matrix to an empty random weight matrix 
        // 1) so that the dimensions of the matrix are set up 
        // 2) we'll leave 10% of the matrix untouched as the random mutation 
        float[][][] babyWeightMatrix = CreateWeightMatrix();


        //let each value in the weight matrix be inherited by parent 1 (in top 5 performers) with 65% chance 
        //or from parent 2 (in top 10 performers) with 25% chance
        //or, as mentioned, leave it and then it'll be random 
        for (var i=0; i < babyWeightMatrix.Length; i++)
        {
          
            for (var j = 0; j < babyWeightMatrix[i].Length; j++)
            {

                float randomChance = Random.Range(0.0f, 1.0f);
                for (var k = 0; k < babyWeightMatrix[i][j].Length; k++)
                {
                    //65% chance from parent 1
                    if (randomChance < 0.65f)
                    {
                        babyWeightMatrix[i][j][k] = parent1[i][j][k];                  
                    }

                    //25% chance from parent 2
                    else if (randomChance < 0.9f)
                    { 
                    babyWeightMatrix[i][j][k] = parent2[i][j][k];
                    }
             
                }
            }
        }
        
        return babyWeightMatrix;
    }


    //use this method to make sure that the players ignore each other and don't collide in the gameplay 
    //needed since we're spawning multiple players at the same time per generation 
    void ignoreOtherPlayers()
    {
        for (var i = 0; i < population.Count; i++)
        {
            for (var j = 0; j < population.Count; j++)
            {
                if (i != j)
                {
                    Physics.IgnoreCollision(population[i].GetComponent<SphereCollider>(), population[j].GetComponent<SphereCollider>());
                }
            }
        }
    }


    void NewPopulation()
    {
        List<float[][][]> newgenerationWeightMatrices = new List<float[][][]>();
        List<float[][][]> matingPool = new List<float[][][]>();
        float[][][] babyMatrix;


        //order population by those who progressed furthest in the game to those who progressed the least
        population = population.OrderByDescending(go => go.GetComponent<neuralNetwork>().getCornersPassedScore()).ThenByDescending(go => go.transform.position.z)
           .ToList();
       
        //save the weight matrix for the best performing player incase we want to end the algorithm and use this as the winning weight matrix
        bestLittleMatrix = population[0].GetComponent<neuralNetwork>().GetWeightsMatrix();

        //keep the top 1/30 who went the furthest and add to mating pool 
        for (var i = 0; i < populationSize / 30; i++)
        {
            matingPool.Add(population[i].GetComponent<neuralNetwork>().GetWeightsMatrix());
        }
       

        //keep the top 2 performers and 
        // 1) add them to the next generation so that we don't lose progress if they were to be mutated
        // 2) mutate 10% without crossover and add these two 'children' to the next generation 
        for (var i = 0; i < 2; i++)
            {
           newgenerationWeightMatrices.Add(population[i].GetComponent<neuralNetwork>().GetWeightsMatrix());
           newgenerationWeightMatrices.Add(CrossOverAndMutate(population[i].GetComponent<neuralNetwork>().GetWeightsMatrix(), population[i].GetComponent<neuralNetwork>().GetWeightsMatrix()));
        }
      

        //add new children to the gene pool by crossing over and mutating parents
        //until we have the same amount of children as we did in the last generation 
         while (newgenerationWeightMatrices.Count < populationSize)
        {
             babyMatrix = CrossOverAndMutate(matingPool[Random.Range((int)0, Mathf.RoundToInt(matingPool.Count/2))], matingPool[Random.Range((int)0, matingPool.Count - 1)]);
             newgenerationWeightMatrices.Add(babyMatrix);
        }
       

         //destroy the current generation of players in the game 
        for (var i = 0; i < populationSize; i++) {
            Destroy(population[i]);
        }
        population.Clear();


        //instantiate the new generation of players
        for (var i = 0; i < populationSize; i++)
        {
            GameObject go = Instantiate(playerPrefab);
         
            go.GetComponent<neuralNetwork>().inheritWeightMatrix(newgenerationWeightMatrices[i]);
            population.Add(go);
        }
    }


    void BestDude()
    {
        //destroy the current population 
         for (var i = 0; i < population.Count; i++)
        {
            Destroy(population[i]);
        }
        population.Clear();

        //instantiate only the best performing player from the last generation 
        GameObject go = Instantiate(playerPrefab);
        go.GetComponent<neuralNetwork>().inheritWeightMatrix(bestLittleMatrix);
        population.Add(go);
    }

}



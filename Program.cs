using System;
using System.Collections.Generic;
using System.Linq;

namespace AG
{
    class Program
    {


        //celem tego algorytmu jest odgadnięcie tego idealnego rozwiązania,
        //którego oczywiście ukrywamy przed algorytmem, algorytm ma dostęp 
        //tylko do funkcji GetFitness(int[] X), która zwraca ilość zgodnych pozycji 
        //między idealnym rozwiązaniem, a danym osobnikiem, czyli wartość fitness 
        //danego osobnika
        //jest to oczywiście tak prosta fitness, jaka nie występuje w rzeczywistych zagadnieniach, 
        //ponieważ nie ma w niej tu żadnych interakcji między poszczególnymi wartościami





        static double[] _slots;

        static private void FillSlots(double[] _fitness)
        {
            int _populationSize = _fitness.Length;
            _slots = new double[_populationSize + 1];
            _slots[0] = _fitness[0];

            for (int p = 1; p < _populationSize; p++)
                _slots[p] = _slots[p - 1] + _fitness[p];
        }


        static private int SearchSlot(double point)
        {
            int _populationSize = _slots.Length;
            int dk;
            int k = (int)(0.5 * _populationSize);

            double m = 0.25;
            if (point <= _slots[0])
                return 0;

            while (!(_slots[k] <= point && _slots[k + 1] > point))
            {
                dk = (int)(m * _populationSize + 0.5);

                if (_slots[k] > point)
                    k -= dk;
                else
                    k += dk;

                m *= 0.5;
            }

            return k;
        }


        static int GetFitness(int[] X)
        {
            int fitness = 0;
            for (int i = 0; i < X.Length; i++)
            {
                if (IdealSolution[i] == X[i])
                    fitness++;
            }
            return fitness;
        }


        static int[] IdealSolution;

        enum OptimizationType
        {
            RouletteSelection,
            TournamentSelection,
            MutationFrequency,
            NumberOfParents
        }



        static void Main(string[] args)
        {
            string liczba = "5";

            while (liczba != "0" && liczba != "1" && liczba != "2" && liczba != "3" && liczba != "4")
            {
                Console.WriteLine("Możliwe parametry do optymalizacji:");
                Console.WriteLine("1 - selekcja ruletkowa");
                Console.WriteLine("2 - selekcja turniejowa");
                Console.WriteLine("3 - mutacja");
                Console.WriteLine("4 - liczba rodziców");
                Console.WriteLine("0 - koniec pracy programu");
                Console.Write("wybierz liczbę (0-4): ");
                liczba = Console.ReadLine();
            }

            OptimizationType optimizationType;
            switch (liczba)
            {
                case "1": optimizationType = OptimizationType.RouletteSelection; break;
                case "2": optimizationType = OptimizationType.TournamentSelection; break;
                case "3": optimizationType = OptimizationType.MutationFrequency; break;
                case "4": optimizationType = OptimizationType.NumberOfParents; break;
                default: return;
            }

            int numRuns = 10;
            int[] chromosomeLengths = { 25, 50, 100, 200, 400, 800 };
            double[] optimizedParametersRouletteSelection = { 0, -0.02, -0.05, -0.10, -0.25, -0.50, -0.75, -1.0, 0.25, 0.50, 0.75, 0.90 };
            int[] optimizedParametersTournamentSelection = { 2, 3, 4, 6, 9, 14, 20, 28, 40, 60, 90, 140, 200 };
            double[] optimizedParametersMutationFrequency = { 0.0, 0.2, 0.28, 0.4, 0.6, 0.9, 1.4, 2.0, 2.8, 4.0, 6.0, 9.0, 14.0, 20.0 };
            int[] optimizedParametersNumParents = { 2, 3, 4, 6, 9, 14, 20, 28, 40, 60, 90, 140, 200 };
            double[] fitnessToLog = { 0.60, 0.70, 0.80, 0.90, 0.95, 0.98, 0.99, 1.0 };

            Console.WriteLine();
            Console.Write("podaj katalog na logi (<Enter> oznacza domyślny katalog 'logs' w bieżącym katalogu)");
            string logDirectory = Console.ReadLine();

            if (logDirectory == "")
                logDirectory = "logs";

            try
            {
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);
            }
            catch
            {
                Console.WriteLine("nie można utworzyć katalogu " + logDirectory + "  logi będą zapisane katalogu 'logs' w bieżącym katalogu");
                logDirectory = "logs";
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);
            }

            string logFile = logDirectory + @"\log.txt";
            int numParamValues = 9;

            switch (optimizationType)
            {
                case OptimizationType.RouletteSelection:
                    logFile = logDirectory + @"\LogRouletteSelection_" + DateTime.Now.Ticks + ".txt";
                    numParamValues = optimizedParametersRouletteSelection.Length;
                    break;
                case OptimizationType.TournamentSelection:
                    logFile = logDirectory + @"\LogTournamentSelection_" + DateTime.Now.Ticks + ".txt";
                    numParamValues = optimizedParametersTournamentSelection.Length;
                    break;
                case OptimizationType.MutationFrequency:
                    logFile = logDirectory + @"\LogMutation_" + DateTime.Now.Ticks + ".txt";
                    numParamValues = optimizedParametersMutationFrequency.Length;
                    break;
                case OptimizationType.NumberOfParents:
                    logFile = logDirectory + @"\LogNumParents_" + DateTime.Now.Ticks + ".txt";
                    numParamValues = optimizedParametersNumParents.Length;
                    break;
            }

            string rs = "chromosomeLength optimizedParameter";
            for (int i = 0; i < fitnessToLog.Length; i++)
                rs += $" {fitnessToLog[i]:F2}maxF";
            for (int i = 0; i < fitnessToLog.Length; i++)
                rs += $" {fitnessToLog[i]:F2}%suc";


            System.IO.File.AppendAllText(logFile, rs + "\r\n");


            for (int ch = 0; ch < chromosomeLengths.Length; ch++)
            {


                for (int op = 0; op < numParamValues; op++)
                {
                    int[][] numEvalsForFitnessToLog = new int[fitnessToLog.Length][];
                    for (int f = 0; f < fitnessToLog.Length; f++)
                        numEvalsForFitnessToLog[f] = new int[numRuns];

                    int populationSize = 100;// minimum populationSize = 10
                    int chromosomeLength = chromosomeLengths[ch];
                    int maxEpoch = 400 + 2*chromosomeLength;
                    int numberOfCandidates = 8;
                    double mutationFrequency = 4.0;
                    int numParents = 8;
                    int elite = (int)(0.1 * populationSize);
                    string selection = "T";

                    if (optimizationType == OptimizationType.TournamentSelection && optimizedParametersTournamentSelection[op] > chromosomeLength)                   
                        break;
                    
                    if (optimizationType == OptimizationType.NumberOfParents && optimizedParametersNumParents[op] > chromosomeLength)
                        break;

                    Random R = new Random();

                    for (int run = 0; run < numRuns; run++)
                    {

                        for (int i = 0; i < fitnessToLog.Length; i++)
                            numEvalsForFitnessToLog[i][run] = -1;

                        IdealSolution = new int[chromosomeLength];                      

                        for (int i = 0; i < chromosomeLength; i++)
                        {
                            IdealSolution[i] = R.Next(2);
                        }

                      

                        int epochsWithoutImprovementInMaxFitness = 0;
                        double previousMaxFitness = 0;

                        switch (optimizationType)
                        {
                            case OptimizationType.RouletteSelection:
                                selection = "R";
                                break;
                            case OptimizationType.TournamentSelection:
                                selection = "T";
                                numberOfCandidates = optimizedParametersNumParents[op];
                                break;
                            case OptimizationType.MutationFrequency:
                                mutationFrequency = optimizedParametersMutationFrequency[op];
                                break;
                            case OptimizationType.NumberOfParents:
                                numParents = optimizedParametersNumParents[op];
                                break;
                        }




                        int[][] population = new int[populationSize][];
                        int[][] childPopulation = new int[populationSize - elite][];
                        for (int i = 0; i < populationSize; i++)
                        {
                            population[i] = new int[chromosomeLength];
                        }
                        for (int i = 0; i < populationSize - elite; i++)
                        {
                            childPopulation[i] = new int[chromosomeLength];
                        }


                        double[] fitness = new double[populationSize];



                        //1.generacja populacji

                        for (int i = 0; i < populationSize; i++)
                        {
                            for (int m = 0; m < chromosomeLength; m++)
                                population[i][m] = R.Next(0, 2);
                        }

                        int numberOfFitnessEvaluations = 0;

                        for (int e = 0; e < maxEpoch; e++)
                        {

                            double maxFitness = 0;
                            //2. getfitness każdego

                            for (int i = 0; i < populationSize; i++)
                            {
                                fitness[i] = GetFitness(population[i]);
                                numberOfFitnessEvaluations++;

                                if (fitness[i] > maxFitness)
                                {
                                    maxFitness = fitness[i];
                                    for (int w = 0; w < fitnessToLog.Length; w++)
                                    {
                                        if (maxFitness >= chromosomeLength * fitnessToLog[w] && numEvalsForFitnessToLog[w][run] == -1)
                                            numEvalsForFitnessToLog[w][run] = numberOfFitnessEvaluations;
                                    }

                                    if (fitness[i] == chromosomeLength)
                                        break;
                                }
                            }

                            if (maxFitness == chromosomeLength)
                                break;


                            if (previousMaxFitness >= maxFitness)
                            {
                                if (epochsWithoutImprovementInMaxFitness++ > 180)
                                {                                   
                                    break;
                                }
                            }
                            else
                                epochsWithoutImprovementInMaxFitness = 0;

                            previousMaxFitness = maxFitness;

                            Array.Sort(fitness, population);

                            for (int i = 0; i < populationSize - elite; i++)
                            {
                                int[] parents = new int[numParents];
                                int[] splitPoints = new int[numParents];
                                for (int p = 1; p < numParents - 1; p++)
                                    splitPoints[p] = R.Next(chromosomeLength);

                                splitPoints[0] = 0;
                                splitPoints[numParents - 1] = chromosomeLength;

                                Array.Sort(splitPoints);

                                if (selection == "T")
                                {
                                    //3. selekcja turniejowa -> rodziców (osobno dla każdego osobnika) -> krzyżowanie -> dziecko

                                    for (int p = 0; p < numParents; p++)
                                    {

                                        int maxK = 0;
                                        double maxKFitness = 0;

                                        for (int k = 0; k < numberOfCandidates; k++)
                                        {
                                            int candidate = R.Next(populationSize);

                                            if (fitness[candidate] > maxKFitness)
                                            {
                                                maxKFitness = fitness[candidate];
                                                maxK = candidate;
                                            }
                                        }
                                        parents[p] = maxK;
                                    }
                                }
                                else
                                {

                                    // selekcja ruletkowa                               
                                    if (optimizedParametersRouletteSelection[op] != 0)
                                    {
                                        double xF = 0;
                                        if (optimizedParametersRouletteSelection[op] > 0)
                                        {
                                            int x = (int)(optimizedParametersRouletteSelection[op] * fitness.Length);

                                            if (x >= 1)
                                            {
                                                int nr = (int)(optimizedParametersRouletteSelection[op] * fitness.Length);
                                                xF = fitness.Take(nr).Max();
                                            }
                                            else
                                                xF = fitness.Min();
                                        }
                                        else if (optimizedParametersRouletteSelection[op] < 0)
                                        {
                                            int nr = (int)(0.1 * fitness.Length);
                                            double mx = fitness.Take(nr).Max();
                                            xF = (-optimizedParametersRouletteSelection[op]) * mx;
                                        }


                                        for (int f = 0; f < fitness.Length; f++)
                                        {
                                            if (fitness[f] > xF)
                                                fitness[f] = (fitness[f] - xF) / maxFitness;
                                            else
                                                fitness[f] = 0;
                                        }
                                    }
                                    else
                                    {
                                        for (int f = 0; f < fitness.Length; f++)
                                        {
                                            fitness[f] /= maxFitness;
                                        }
                                    }

                                    double sumFitness = fitness.Sum();
                                    FillSlots(fitness);

                                    for (int p = 0; p < numParents; p++)
                                    {
                                        parents[p] = SearchSlot(sumFitness * R.NextDouble());
                                    }
                                }


                                for (int s = 0; s < numParents - 1; s++)
                                {
                                    for (int m = splitPoints[s]; m < splitPoints[s + 1]; m++)
                                    {
                                        childPopulation[i][m] = population[parents[s]][m];
                                    }
                                }
                            }


                            for (int i = 0; i < populationSize - elite; i++)
                            {
                                for (int m = 0; m < chromosomeLength; m++)
                                    population[i][m] = childPopulation[i][m];
                            }


                            for (int i = 0; i < populationSize; i++)
                            {
                                if (R.NextDouble() < 0.1 * mutationFrequency)
                                {
                                    int rm = R.Next((int)(mutationFrequency + 1));
                                    for (int x = 0; x < rm; x++)
                                    {
                                        int xi = R.Next(chromosomeLength);
                                        if (population[i][xi] == 0)
                                            population[i][xi] = 1;
                                        else
                                            population[i][xi] = 0;
                                    }
                                }
                            }


                        }
                        //   Console.WriteLine($"chromosomeLength={chromosomeLength}  run={run}/{numRuns} numEval={NumEval[run]}");
                        //   Console.Write(run + " ");
                    }
                    // Console.WriteLine();
                    //  Console.WriteLine("avgNumEvals(chromosomeLength={chromosomeLength})=" + NumEval.Average());
                    //  Console.WriteLine();

                    double paramValue;
                    switch (optimizationType)
                    {
                        case OptimizationType.RouletteSelection:
                            paramValue = optimizedParametersRouletteSelection[op];
                            break;
                        case OptimizationType.TournamentSelection:
                            paramValue = optimizedParametersTournamentSelection[op];
                            break;
                        case OptimizationType.MutationFrequency:
                            paramValue = optimizedParametersMutationFrequency[op];
                            break;
                        default:
                            paramValue = optimizedParametersNumParents[op];
                            break;
                    }
                    double[] numEvalAverage = new double[fitnessToLog.Length];
                    double[] successPercentage = new double[fitnessToLog.Length];
                    double[] sr = new double[fitnessToLog.Length];
                    string res = "";
                    for (int i = 0; i < fitnessToLog.Length; i++)
                    {
                        int numMinus1 = 0;
                        List<double> numSuccessfullEvalsForFitnessToLog = new List<double>();
                        for (int r = 0; r < numRuns; r++)
                        {
                            if (numEvalsForFitnessToLog[i][r] == -1)
                                numMinus1++;
                            else
                                numSuccessfullEvalsForFitnessToLog.Add(numEvalsForFitnessToLog[i][r]);
                        }
                        successPercentage[i] = 100 * (1 - (double)numMinus1 / numRuns);

                        if (numSuccessfullEvalsForFitnessToLog.Count > 0)
                            sr[i] = numSuccessfullEvalsForFitnessToLog.Average();
                        else
                            sr[i] = -1;

                        res += $" {sr[i]:F1}";
                    }
                    for (int i = 0; i < fitnessToLog.Length; i++)
                        res += $" {successPercentage[i]:F0}";


                    Console.WriteLine($"chromosomeLength={chromosomeLengths[ch]} optimizedParameter={paramValue} numFitnessEval={sr[fitnessToLog.Length - 1]:F0} success%={successPercentage[fitnessToLog.Length - 1]:F0}");

                    System.IO.File.AppendAllText(logFile, chromosomeLengths[ch] + " " + paramValue + res + "\r\n");
                }//op
                Console.WriteLine();
            }//ch

            Console.WriteLine();
            Console.WriteLine("Optymalizacja zakończona.");
            Console.WriteLine("Logi znajdują się w pliku " + logFile + ".");
            Console.WriteLine("Proszę teraz w Excelu wygenerować z tych logów wykresy (powinny zawierać kolumny B oraz J dla danych wartości w kolumnie A) oraz przedstawić wnioski.");
            Console.WriteLine("W poszczególnych kolumnach jest to co pisze w nagłówkach, czyli kolejno: gługość chromosomu, wartość optymalizowanego parametru, liczba ewaluacji funkcji fitness do uzyskania przez najlepszego osobnika odopowiedino 60%, 70%, 80%, 90%, 95%, 98%, 99% i 100% wartości maksymalnej fitness, a w kolejnych kolumnach ile procent optymalizacji zakończyło się sukcesem");
            Console.WriteLine("Wartości średnie są w logach liczone tylko z udanych przebiegów. Wartość -1  oznacza, że w tym przypadku w ani jednej próbie nie osiągnięto wyniku w założonej liczbie iteracji.");
            Console.WriteLine();
            Console.WriteLine();



        }


    }
}
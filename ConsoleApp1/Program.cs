using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Console;
using static System.Math;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


namespace QLearning
{
  class QLearningProgram
  {
    static Random rnd = new Random();
    static void Main(string[] args)
    {
      Console.WriteLine("Begin Q-learning maze demo");
      Console.WriteLine("Setting up maze and rewards");
      int ns = 20;
      (int[][] FT, double[][] R) = CreateMazeAndReward(ns);
      double[][] Q = CreateQuality(ns);
      Console.WriteLine("Analyzing maze using Q-learning");
      int goal = ns-1;
      double gamma = 0.5;
      double learnRate = 0.5;
      int maxEpochs = 10;
      Train(FT, R, Q, goal, gamma, learnRate, maxEpochs);
      Console.WriteLine("Done. Q matrix: ");
      Print(Q);
      int i = rnd.Next(0, FT.Length-1);
      Console.WriteLine($"Using Q to walk from cell {i} to {goal}");
      Walk(i, goal, Q);
      Console.WriteLine("End demo");
      Console.ReadLine();
    }
    static int NextFT(int s, int ns, int[][] FT, double[][] R)
    {
      int k = rnd.Next(0, ns - 1);
      while (k == s)
      {
        k = rnd.Next(0, ns - 1);
      }
      FT[s][k] = 1;
      R[s][k] = 1;
      return k;

    }
    static (int[][], double[][]) CreateMazeAndReward(int ns)
    {
      int[][] FT = new int[ns][];
      double[][] R = new double[ns][];
      for (int i = 0; i < ns; ++i) FT[i] = new int[ns];
      for (int i = 0; i < ns; ++i) R[i] = new double[ns];
      int k = 0;
      for (int j=0; j<ns;++j)
      {
        k = NextFT(k, ns, FT, R);
        Console.Write(k);
        if (j == ns - 1) { FT[k][ns - 1] = 1; FT[ns - 1][ns - 1] = 1; R[k][ns - 1] = 10; }
      }
      Console.WriteLine();
      return (FT, R);
    }
    static double[][] CreateReward(int ns)
    {
      double[][] R = new double[ns][];
      for (int i = 0; i < ns; ++i) R[i] = new double[ns];

      R[ns-1][ns-1] = 10.0;  // Goal
      return R;
    }
    static double[][] CreateQuality(int ns)
    {
      double[][] Q = new double[ns][];
      for (int i = 0; i < ns; ++i)
        Q[i] = new double[ns];
      return Q;
    }

    static List<int> GetPossNextStates(int s, int[][] FT)
    {
      List<int> result = new List<int>();
      Parallel.For(0, FT.Length, j =>
      {
        if (FT[s][j] == 1) result.Add(j);
      });
      if (result.Count == 0) { result.Add(0); return result; } else { return result; }
    }
    static int GetRandNextState(int s, int[][] FT)
    {
      List<int> possNextStates = GetPossNextStates(s, FT);
      int ct = possNextStates.Count;
      int idx = rnd.Next(0, ct);
      return possNextStates[idx];
    }

    static void Train(int[][] FT, double[][] R, double[][] Q,
    int goal, double gamma, double lrnRate, int maxEpochs)
    {
      for(int j = 0; j<maxEpochs; ++j)
      {
        int currState = rnd.Next(0, R.Length);
        while (true)
        {
          int nextState = GetRandNextState(currState, FT);
          List<int> possNextNextStates = GetPossNextStates(nextState, FT);
          double maxQ = double.MinValue;
          Parallel.For(0, possNextNextStates.Count, j =>
          {
            int nns = possNextNextStates[j];  // short alias
            double q = Q[nextState][nns];
            if (q > maxQ) maxQ = q;
          });
          Q[currState][nextState] =
          ((1 - lrnRate) * Q[currState][nextState]) +
          (lrnRate * (R[currState][nextState] + (gamma * maxQ)));
          currState = nextState;
          if (currState == goal) break;
        } // while
      } // for
    } // Train

    static int ArgMax(double[] vector)
    {
      double maxVal = vector[0]; int idx = 0;
      Parallel.For(0, vector.Length, i =>
      {
        if (vector[i] > maxVal)
        {
          maxVal = vector[i]; idx = i;
        }
      });
      return idx;
    }

    static void Print(double[][] Q)
    {
      int ns = Q.Length;
      Console.WriteLine("PRINTING Q MATRIX");
      for (int i = 0; i < ns; ++i)
      {
        for (int j = 0; j < ns; ++j)
        {
          Console.Write(Q[i][j].ToString("F2") + " ");
        }
        Console.WriteLine();
      }
    }

    static void Walk(int start, int goal, double[][] Q)
    {
      int curr = start; int next;
      Console.Write(curr + "->");
      while (curr != goal)
      {
        next = ArgMax(Q[curr]);
        Console.Write(next + "->");
        curr = next;
      }
      Console.WriteLine("done");
    }

  } // Program


}// ns






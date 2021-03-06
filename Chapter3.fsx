﻿// ========================================================
// Chapter 3: Implementing Machine Learning Algorithms
// ========================================================


// --------------------------------------------------------
// Clustering 2D Points
// --------------------------------------------------------

// First, we use sample points in 2D space as inputs
let data =
  [ (0.0, 1.0); (1.0, 1.0);
    (10.0, 1.0); (13.0, 3.0);
    (4.0, 10.0); (5.0, 8.0) ]

// We need to measure distance & aggregate points
let distance (x1, y1) (x2, y2) : float =
  sqrt ((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2))

let aggregate points : float * float =
  (List.averageBy fst points, List.averageBy snd points)

// Initialize centroids randomly (by picking existing points)
let clusterCount = 3
let centroids =
  let random = System.Random()
  [ for i in 1 .. clusterCount ->
      List.nth data (random.Next(data.Length)) ]

// Find the closest centroid for a given input
let closest centroids input =
  centroids
  |> List.mapi (fun i v -> i, v)
  |> List.minBy (fun (_, cent) -> distance cent input)
  |> fst

// Now we can find the closest centroid for each input:
data |> List.map (fun point ->
  closest centroids point)

// Updating clusters recursively
let rec update assignment =
  let centroids =
    [ for i in 0 .. clusterCount-1 ->
        let items =
          List.zip assignment data
          |> List.filter (fun (c, data) -> c = i)
          |> List.map snd
        aggregate items ]
  let next = List.map (closest centroids) data
  if next = assignment then assignment
  else update next

// Run the k-means clustering algorithm on our sample
// 2D points (if this fails, rerun the earlier code that 
// generates random centroids).
let assignment =
  update (List.map (closest centroids) data)


// --------------------------------------------------------
// Writing a Reusable Clustering Function
// --------------------------------------------------------

/// Implementation of the k-means clustering algorithm.
/// The function takes 4 input parameters:
///
///  - `distance` is a function that measures distance 
///    between inputs in the data set
///  - `aggregate` is a function that calculates a 
///    centroid from a collection of data points
///  - `clusterCount` - the required number of clusters
///  - `data` is an array with inputs
///
/// The function returns assignments of inputs into clusters.
let kmeans distance aggregate clusterCount data =
  let centroids =
    let rnd = System.Random()
    [ for i in 1 .. clusterCount ->
      List.nth data (rnd.Next(data.Length)) ]

  let closest centroids input =
    centroids
    |> List.mapi (fun i v -> i, v)
    |> List.minBy (fun (_, cent) -> distance cent input)
    |> fst

  let rec update assignment =
    let centroids =
      [ for i in 0 .. clusterCount-1 ->
          let items =
            List.zip assignment data
            |> List.filter (fun (c, data) -> c = i)
            |> List.map snd
          aggregate items ]

  let next = List.map (closest centroids) data
  if next = assignment then assignment
  else update next

// Run k-means clustering on 2D points again
update (List.map (closest centroids) data)


// --------------------------------------------------------
// Clustering Countries
// --------------------------------------------------------

// We are in a new file, but we want to access some values
// defined in Chapter 2 - to do this, we can run all the 
// code in Chapter 2 using '#load'. We also need Deedle &
// XPlot to do the charting

#load "Chapter2.fsx"
open Chapter2
open Deedle
open XPlot.GoogleCharts

// Input for the clustering will be rows of the frame
// (representing individual countries in the world)
let data =
  norm.GetRows<float>().Values |> List.ofSeq

// The distance and aggregate functions now perform
// calculations over the whole series (with different
// indicators)
let distance
    (s1:Series<string,float>)
    (s2:Series<string, float>) =
  (s1 - s2) * (s1 - s2) |> Stats.sum

let aggregate items =
  items
  |> Frame.ofRowsOrdinal
  |> Stats.mean

// Visualize the results using 3 different colors
// for the 3 different clusters of countries
let clrs = ColorAxis(colors=[|"red";"blue";"orange"|])
let countryClusters =
  kmeans distance aggregate 3 data

Seq.zip norm.RowKeys countryClusters
|> Chart.Geo
|> Chart.WithOptions(Options(colorAxis=clrs))
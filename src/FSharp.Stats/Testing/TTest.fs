namespace FSharp.Stats.Testing


module TTest =

    open FSharp.Stats

    /// Equal or unequal sample sizes, assume nothing about variance.
    /// input: (mean1,variance1,N1) (mean2,variance2,N3)
    let private noAssumtion (m1,s1,n1:float) (m2,s2,n2:float) =        
        let sd = System.Math.Sqrt(s1 / n1 + s2 / n2);
        let statistic = (m1 - m2) / sd
        let r1  = s1 / n1
        let r2  = s2 / n2
        let dof = (((r1 + r2) * (r1 + r2)) / ((r1 * r1) / (n1 - 1.) + (r2 * r2) / (n2 - 1.)));
    
        TestStatistics.createTTest statistic dof

    let twoSampleFromMeanAndVar (assumeEqualVariances:bool) (mean1,variance1,n1) (mean2,variance2,n2) =

        let equalSampleSize = n1 = n2
        if (assumeEqualVariances) then
                if (equalSampleSize) then
                        // Samples have the same size and assume same variance.
                        let sp = System.Math.Sqrt(0.5 * (variance1 + variance2))
                        let statistic = (mean1 - mean2) / (sp * System.Math.Sqrt(2.0 / n1))
                        let dof = 2. * n1 - 2.
                        TestStatistics.createTTest statistic dof 
                else                
                    // Samples have unequal sizes, but assume same variance.
                    //let sp = 4.0//Statistics.Tools.PooledVariance(sample1, sample2);
                    let sp = (variance1 * (n1-1.) + variance2 * (n2-1.)) / (n1 + n2 - 2.)
                    let statistic = (mean1 - mean2) / ((sqrt sp) * System.Math.Sqrt(1.0 / n1 + 1.0 / n2));
                    
                    let dof = n1 + n2 - 2.
                    TestStatistics.createTTest statistic dof
        else
            // Unequal sample sizes, assume nothing about variance.
            noAssumtion (mean1,variance1,n1) (mean2,variance2,n2)      

    /// Computes a t-test or a Welch test (unequal variances)
    let twoSample (assumeEqualVariances:bool) sample1 sample2 =
        let s1Stats = Vector.stats sample1
        let s2Stats = Vector.stats sample2

        let v1 = SummaryStats.var s1Stats
        let v2 = SummaryStats.var s2Stats

        twoSampleFromMeanAndVar assumeEqualVariances (s1Stats.Mean,v1,s1Stats.N) (s2Stats.Mean,v2,s2Stats.N)

    //// Computes a one sample t-test used to compare a population mean with a given value (mu).
    let oneSample (sample1:Vector<float>) mu =
        let n = float sample1.Length 
        let sampleMean = Seq.mean sample1
        let sampleStDev = Seq.stDev sample1
        let statistic = (sampleMean - mu) / (sampleStDev / (sqrt n))
        TestStatistics.createTTest statistic (n - 1.)  

    //// Computes a one sample t-test used to compare a population mean with a given value (mu).
    let oneSampleFromMeanAndStDev (mean,stDev,n) mu =
        let statistic = (mean - mu) / (stDev / (sqrt n))
        TestStatistics.createTTest statistic (n - 1.)  

    //// Computes a paired t-test used to compare two population means where you have two samples in
    /// which observations in one sample can be paired with observations in the other sample.
    let twoSamplePaired (sample1:Vector<float>) (sample2:Vector<float>) =                
        let deltas = Vector.map2 (fun a b -> b - a) sample1 sample2          
        oneSample deltas 0.


        
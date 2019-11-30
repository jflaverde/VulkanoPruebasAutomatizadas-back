﻿using DataFramework.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ControllerVulkano;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace WorkerWebsiteBDT
{
    class Program
    {
        static void Main()
        {
            GeneralWorkerNetFramework.RabbitMQ rabbitMQ = new GeneralWorkerNetFramework.RabbitMQ();
            EstrategiaDTO strategy = rabbitMQ.GetMessages("QUEUE_BDT");

            for (int i = 0; i < strategy.TipoPruebas.First().CantidadEjecuciones; i++)
            {
                StartProcess(strategy);
            }
        }

        /// <summary>
        /// Start with the process of environment preparation and test execution
        /// </summary>
        /// <param name="strategy"><see cref="EstrategiaDTO"/></param>
        static void StartProcess(EstrategiaDTO strategy)
        {
            

            try
            {
                WorkerStatus worker = new WorkerStatus();
                worker.WorkerID = Convert.ToInt32(ConfigurationManager.AppSettings["WorkerID"]);
                worker.Estado.ID = 6;
                worker.TipoPrueba = "QUEUE_BDT";
                WorkerController workerController = new WorkerController();
                workerController.UpdateWorkerStatus(worker);

                foreach (TipoPruebaDTO tipoPrueba in strategy.TipoPruebas)
                {
                    TipoPruebaController tipoPruebaController = new TipoPruebaController();
                    ScriptFile scriptFile = new ScriptFile();


                    //guarda en el historico de ejecucion de pruebas
                    int idExecution = tipoPruebaController.InsertEjecucionTipoPrueba(strategy.Estrategia_ID, strategy.TipoPruebas[0].ID, 0, "", EstadoEnum.EnEjecucion);

                    //ruta donde se encuentra el archivo del script
                    string testProject = Path.Combine(scriptFile.GetScriptProjectPath(), ConfigurationManager.AppSettings["RutaScript"]);
                    //ruta destino temporal
                    string destinationFolder = @"C:\Temp";
                    string destinationPath = string.Concat(destinationFolder, @"\", tipoPrueba.Script.Script);
                    //ruta de origen del script
                    string scriptPath = string.Concat(testProject, tipoPrueba.Script.Script, tipoPrueba.Script.Nombre, tipoPrueba.Script.Extension);

                    //si la ruta destino no existe se crea
                    if (!File.Exists(destinationPath))
                    {
                        Directory.CreateDirectory(destinationPath);
                    }

                    // Extract the zip file
                    GeneralWorkerNetFramework.ActionsFile actionsFile = new GeneralWorkerNetFramework.ActionsFile();
                    actionsFile.UnzipFile(scriptPath, destinationPath);

                    // Create json structure file
                    //string projectPath = //@"F:\Universidad de los Andes\MISO\Pruebas Automaticas\T Grupal\-201920_MISO4208\PrestashopTest";
                    string dataDestinationFolder = Path.Combine(destinationPath, @"Prestashop.Core\fixtures\data.json");
                    //parametros de mockaroo
                    #region Mockaroo
                    ParametersRequest parameters = new ParametersRequest
                    {
                        ApiController = tipoPrueba.ApiController,
                        Key = tipoPrueba.ApiKey
                    };

                    JsonFile jsonFile = new JsonFile();
                    int numberDataGenerated = jsonFile.GenerateData(parameters, dataDestinationFolder);
                    #endregion
                    // Reemplaza tokens del script
                    #region Reemplazar tokens
                    IEnumerable<string> featureFiles = scriptFile.GetFeatureFiles(destinationPath);
                    // Replace tokens with random position object of json file
                    foreach (string featureFile in featureFiles)
                    {
                        scriptFile.ReplaceTokens(featureFile, numberDataGenerated);
                    }
                    #endregion
                    // Executes Cypress script
                    if (strategy != null)
                    {
                        string package = Path.Combine(destinationPath, tipoPrueba.Script.Nombre, "BDT.TestProject");
                        // Install the node modules for each of the test project

                        var psiNpmRunDist = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            RedirectStandardInput = true,
                            UseShellExecute = false,
                            WorkingDirectory = Directory.GetParent(package).FullName


                        };

                        var pNpmRunDist = Process.Start(psiNpmRunDist);
                        StringBuilder output = new StringBuilder();
                        pNpmRunDist.StandardInput.WriteLine("npm i");
                        Thread.Sleep(1000);

                        pNpmRunDist.StandardInput.WriteLine("npx cypress run test " + tipoPrueba.Parametros);
                        pNpmRunDist.WaitForExit();
                        //marca como finalizado
                        tipoPruebaController.InsertEjecucionTipoPrueba(strategy.Estrategia_ID, strategy.TipoPruebas[0].ID, idExecution, "", EstadoEnum.Finalizado);
                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine("error " + ex);
            }
            finally
            {
                WorkerStatus worker = new WorkerStatus();
                worker.WorkerID = Convert.ToInt32(ConfigurationManager.AppSettings["WorkerID"]);
                worker.Estado.ID = 5;
                worker.TipoPrueba = "QUEUE_BDT";
                WorkerController workerController = new WorkerController();
                workerController.UpdateWorkerStatus(worker);
            }

            
        }
    }
}
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FACE
{
    class Program
    {
        FaceServiceClient caraService = new FaceServiceClient("88aeb72629714eae9ef4090255b08c61", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");
        public async void CrearGrupoPersona(string grupoPersonaId, string nombrePersonaGrupo)
        {
            try
            {
                await caraService.CreatePersonGroupAsync(grupoPersonaId, nombrePersonaGrupo);
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR CREANDO PERSONA EN GRUPO\n"+ex.Message);
            }
        }
        public async void AnadirPersona(string grupoPersonaId, string nombre, string path)
        {
            try
            {
                await caraService.GetPersonGroupAsync(grupoPersonaId);
                CreatePersonResult persona = await caraService.CreatePersonAsync(grupoPersonaId, nombre);
                RegistrarCara(grupoPersonaId, persona,path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR ANADIR PERSONA\n" + ex.Message);
            }
        }
        private async void RegistrarCara(string grupoPersonaId, CreatePersonResult persona, string path)
        {
            foreach(var img in Directory.GetFiles(path, "*.jpg"))
            {
                using (Stream s = File.OpenRead(img))
                {
                    await caraService.AddPersonFaceAsync(grupoPersonaId, persona.PersonId, s);
                }
            }
        }
        public async void entrenar(string grupoPersonaId)
        {
            await caraService.TrainPersonGroupAsync(grupoPersonaId);
            TrainingStatus estado = null;
            while(true)
            {
                estado = await caraService.GetPersonGroupTrainingStatusAsync(grupoPersonaId);
                if (estado.Status != Status.Running)
                    break;
                await Task.Delay(1000);
            }
            Console.WriteLine("Entrenamiento Completo");
        }
        public async void ReconocerCara(string grupoPersonaId, string path)
        {
            using (Stream s = File.OpenRead(path))
            {
                var cara = await caraService.DetectAsync(s);
                var caraId = cara.Select(c => c.FaceId).ToArray();
                try
                {
                    var resultado = await caraService.IdentifyAsync(grupoPersonaId, caraId);
                    foreach (var identificar in resultado)
                    {
                        Console.WriteLine(identificar.FaceId);
                        if (identificar.Candidates.Length==0)
                        {
                            Console.WriteLine("NO HAY RESULTADOS");
                        }
                        else
                        {
                            var idCandidato = identificar.Candidates[0].PersonId;
                            var persona = await caraService.GetPersonAsync(grupoPersonaId, idCandidato);
                            Console.WriteLine(persona.Name);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("ERROR IDENTIFICAR PERSONA\n" + ex.Message);

                }
            }
        }
        static void Main(string[] args)
        {
            try
            {
                //new Program().CrearGrupoPersona("hollywoodstar", "Hollywood Star");
                //new Program().AnadirPersona("hollywoodstar", "Alejandro", @"D:\Imágenes\Faces\Alejandro\");
                new Program().entrenar("hollywoodstar");
                new Program().ReconocerCara("hollywoodstar", @"D:\Imágenes\Faces\Alejandro\Alejandro.jpg");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR MAIN");
            }
        }
    }
}

﻿using DataFramework.DTO;
using DataFramework.Messages;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DataFramework.CRUD
{
    public class ScriptBehavior:ConexionDB
    {
        /// <summary>
        /// Inserta el script a partir de una prueba
        /// </summary>
        /// <param name="tipoPrueba"></param>
        /// <returns></returns>
        public ReturnMessage AddScript(ScriptDTO script)
        {
            ReturnMessage mensaje = new ReturnMessage();
            string query = @"INSERT INTO [dbo].[SCRIPT]
                                   ([NOMBRE]
                                   ,[SCRIPT]
                                   ,[EXTENSION])
                             VALUES
                                   (@Nombre
                                   ,@Script
                                   ,@Extension)

                            SELECT @@IDENTITY AS 'Identity'";

            using (var con = ConectarDB())
            {
                con.Open();

                try
                {
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.Add(new SqlParameter("@Nombre", script.Nombre));
                        command.Parameters.Add(new SqlParameter("@Script", string.Empty));
                        command.Parameters.Add(new SqlParameter("@Extension", string.Empty));
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                script.ID = Convert.ToInt32(reader[0]);
                            }
                        }
                    }

                    mensaje.Mensaje = "El Script se creó correctamente";
                    mensaje.TipoMensaje = TipoMensaje.Correcto;
                    mensaje.obj = script;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Count not insert.");
                    mensaje.Mensaje = ex.Message;
                    mensaje.TipoMensaje = TipoMensaje.Error;
                    mensaje.obj = script;
                }
                finally
                {
                    con.Close();
                }
                return mensaje;
            }
        }

        /// <summary>
        /// Actualizar el script
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public ReturnMessage UpdateScript(ScriptDTO script)
        {
            ReturnMessage mensaje = new ReturnMessage();
            StringBuilder query = new StringBuilder();


            int flagUpdate = 0;
            query.Append(@"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
                            UPDATE SCRIPT SET");

            //valida que los campos tengan un valor para actualizar.
            if (!string.IsNullOrEmpty(script.Script))
            {
                query.Append(" Script=@script");
                flagUpdate = 1;
            }
            if (!string.IsNullOrEmpty(script.Extension))
            {
                if(flagUpdate==1)
                {
                    query.Append(",");
                }
                query.Append(" Extension=@extension");
                flagUpdate = 1;
            }
        

            if (flagUpdate == 1)
            {
                query.Append(" WHERE SCRIPT_ID=@script_id;");
            }

            if (flagUpdate == 1)
            {
                using (var con = ConectarDB())
                {
                    con.Open();

                    try
                    {
                        using (SqlCommand command = new SqlCommand(query.ToString(), con))
                        {
                            if (!string.IsNullOrEmpty(script.Script))
                            {
                                command.Parameters.Add(new SqlParameter("@script", script.Script));
                            }
                            if (!string.IsNullOrEmpty(script.Extension))
                            {
                                command.Parameters.Add(new SqlParameter("@extension", script.Extension));
                            }
                            if(flagUpdate==1)
                            {
                                command.Parameters.Add(new SqlParameter("@script_id", script.ID));
                            }
                           
                            command.ExecuteNonQuery();
                        }
                        mensaje.Mensaje = "El script se actualizó correctamente";
                        mensaje.TipoMensaje = TipoMensaje.Correcto;
                        mensaje.obj = script;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Count not insert.");
                        mensaje.Mensaje = ex.Message;
                        mensaje.TipoMensaje = TipoMensaje.Error;
                        mensaje.obj = script;
                    }
                    finally
                    {
                        con.Close();
                    }
                }


            }
            return mensaje;
        }

        /// <summary>
        /// Selecciona un script dado un tipo de prueba.
        /// </summary>
        /// <param name="tipoPrueba_id"></param>
        /// <returns></returns>
        public ScriptDTO SelectScript(int tipoPrueba_id)
        {
            StringBuilder query = new StringBuilder().Append(@"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
                            SELECT 
								SCRIPT.SCRIPT_ID,
								SCRIPT.NOMBRE,
								SCRIPT.SCRIPT,
								SCRIPT.EXTENSION 
							FROM SCRIPT INNER JOIN TIPOPRUEBA ON SCRIPT.SCRIPT_ID=TIPOPRUEBA.SCRIPT_ID
							AND TIPOPRUEBA.TIPOPRUEBA_ID=@tipoPrueba_id");

            using (var con = ConectarDB())
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(query.ToString(), con))
                    {
                        command.Parameters.Add(new SqlParameter("@tipoPrueba_id", tipoPrueba_id));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ScriptDTO script = new ScriptDTO()
                                {
                                    ID = Convert.ToInt32(reader[0]),
                                    Nombre = reader[1].ToString(),
                                    Script = reader[2].ToString(),
                                    Extension = reader[3].ToString()
                                };
                                return script;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Count not insert.");
                }
                finally
                {
                    con.Close();
                }
            }
            return new ScriptDTO();
        }
    }
}

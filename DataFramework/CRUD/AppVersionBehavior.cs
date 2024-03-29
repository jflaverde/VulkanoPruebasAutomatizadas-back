﻿using System;
using System.Collections.Generic;
using System.Text;
using DataFramework.DTO;
using System.Data.SqlClient;
using DataFramework.Messages;
using System.Linq;
using DataFramework;

namespace Data.CRUD
{
    public class AppVersionBehavior:ConexionDB
    {
        /// <summary>
        /// Crea una estrategia
        /// </summary>
        /// <param name="aplicacion"></param>
        public ReturnMessage CreateAppVersion(AppVersionDTO appVersion)
        {
            ReturnMessage mensaje = new ReturnMessage();
            string query = @"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
                            INSERT INTO APPVERSION (APLICACION_ID, NUMERO,RUTA_APLICACION) VALUES (@APLICACION_ID,@NUMERO,@RUTA_APLICACION)

                            SELECT @@IDENTITY AS 'Identity';";

            using (var con = ConectarDB())
            {
                con.Open();

                try
                {
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.Add(new SqlParameter("@APLICACION_ID", appVersion.Aplicacion_id));
                        command.Parameters.Add(new SqlParameter("@NUMERO", appVersion.Numero));
                        command.Parameters.Add(new SqlParameter("@RUTA_APLICACION", appVersion.Ruta_Aplicacion));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                appVersion.AppVersion_id = Convert.ToInt32(reader[0]);
                            }
                        }
                    }

                    mensaje.Mensaje = "La version se creó correctamente";
                    mensaje.TipoMensaje = TipoMensaje.Correcto;
                    mensaje.obj = appVersion;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("No se pudo insertar");
                    mensaje.Mensaje = ex.Message;
                    mensaje.TipoMensaje = TipoMensaje.Error;
                    mensaje.obj = appVersion;
                }
                finally
                {
                    con.Close();
                }
                return mensaje;
            }

        }


        public List<AppVersionDTO> SelectAppVersion(int aplicacion_id)
        {
            List<AppVersionDTO> listaAppVersiones = new List<AppVersionDTO>();
            StringBuilder query = new StringBuilder().Append(@"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
							SELECT 
							ID,
							APLICACION_ID,
                            NUMERO,
                            RUTA_APLICACION
							FROM APPVERSION");

            if (aplicacion_id != 0)
            {
                query.Append(" WHERE APLICACION_ID=@APLICACION_ID");
            }

            using (var con = ConectarDB())
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(query.ToString(), con))
                    {
                        if (aplicacion_id != 0)
                            command.Parameters.Add(new SqlParameter("@APLICACION_ID", aplicacion_id));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                AppVersionDTO appVersion = new AppVersionDTO()
                                {
                                    AppVersion_id = Convert.ToInt32(reader[0]),
                                    Aplicacion_id = reader[1].ToString(),
                                    Numero = reader[2].ToString(),
                                    Ruta_Aplicacion=reader[3].ToString()
                                };
                                listaAppVersiones.Add(appVersion);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("No se encontró el registro.");
                }
                finally
                {
                    con.Close();
                }
            }
            return listaAppVersiones;
        }
    }
}

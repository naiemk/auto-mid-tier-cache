using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Common;

namespace Common.Data.Sql
{
    public class Dal
    {



        private readonly ILogger _logger;
        private readonly IProgress _progress;
        public const string DefaultConnStr = "Data Source=(local);Initial Catalog=DBLP; Integrated Security=SSPI";

        public Dal(ILogger logger, IProgress progress)
        {
            _logger = logger;
            _progress = progress;
        }

        public DataTable GetDataTable(string connStr, string query)
        {
            _logger.Log(String.Format("Getting data table for quert: {0}", query));
            return RunCommandWithParameter(connStr, query, new SqlParameter[] {}, c =>
                                                                                      {
                                                                                        var da = new SqlDataAdapter(c);
                                                                                        var dt = new DataTable();
                                                                                        da.Fill(dt);
                                                                                        return dt;
                                                                                      });
        }

        public void InsertOrUpdateEntity(string connStr, string insertQ, string updateQ, string existanceCheckQ, Dictionary<string, object> pars)
        {
            var parameters = pars.Select(p => new SqlParameter(p.Key, p.Value));
            if (RunCommandWithParameter(connStr, existanceCheckQ, parameters, c => ((int)c.ExecuteScalar()) > 0))
            {
                _logger.Log(String.Format("Inserting for {0} = {1}", parameters.First().IfNotNull(p => p.ParameterName), parameters.First().IfNotNull(p => p.Value)));
                RunCommandWithParameter(connStr, updateQ, parameters, c => c.ExecuteNonQuery());
            }
            else
            {
                _logger.Log(String.Format("Updating for {0} = {1}", parameters.First().IfNotNull(p => p.ParameterName), parameters.First().IfNotNull(p => p.Value)));
                RunCommandWithParameter(connStr, insertQ, parameters, c => c.ExecuteNonQuery());
            }
        }


        
        public void RunCommandWithParameter(string connStr, string commandQ, Dictionary<string, object> pars = null)
        {
            _logger.Log("Running command" + commandQ);

            var parameters = (pars ?? new Dictionary<string, object>{}).Select(p => new SqlParameter(p.Key, p.Value));
            RunCommandWithParameter(connStr, commandQ, parameters, c => c.ExecuteNonQuery());
        }

        private static T RunCommandWithParameter<T>(string connStr, string commandQ, IEnumerable<SqlParameter> parameters, Func<SqlCommand, T> comRun)
        {
            var conn = new SqlConnection(connStr);
            var comm = new SqlCommand(commandQ, conn);
            comm.Parameters.AddRange(parameters.ToArray());
            conn.Open();
            var rv = comRun(comm);
            conn.Close();
            return rv;
        }

        public void RunForAll<T>(string connStr, string query, IEnumerable<T> data, Func<T, Dictionary<string, object>> func)
        {
            _logger.Log(String.Format("Inserting a buch of data using {0}", query));
            var conn = new SqlConnection(connStr);
            var comm = new SqlCommand(query, conn);
            conn.Open();
            
            _progress.Reset(data.Count());
            foreach (var parameters in data.Select(d => func(d).Select(p => new SqlParameter(p.Key, p.Value))))
            {
                comm.Parameters.Clear();
                comm.Parameters.AddRange(parameters.ToArray());
                comm.ExecuteNonQuery();
                _progress.Progressed(1);
            }
            conn.Close();
            _progress.Finish();
            
            _logger.Log("Finished inserting bunch of data.");
            return;
        }

        public string InsertAllInTempFile<T>(IEnumerable<T> data, Func<T, string > func)
        {
            var fName =System.IO.Path.GetTempFileName();
            using(var tmpF = System.IO.File.CreateText(fName))
            {
                _logger.Log("Startet writing temp file " + fName);
                _progress.Reset(data.Count());
                foreach (var line in data.Select(func))
                {
                    _progress.Progressed(1);
                    tmpF.WriteLine(line);
                } 
                _progress.Finish();
                tmpF.Close();
                return fName;
            }

        }
    }
}

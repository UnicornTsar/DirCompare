using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleTestApp
{

    using FileData = Tuple<string, long, DateTime>;

    public class WorkerException : ArgumentException
    {
        public WorkerException(string msg) : base(msg) { }
    }

    public class WorkerResult
    {
        private IEnumerable<string> matching;
        private IEnumerable<string> different;
        public WorkerResult(IEnumerable<string> m, IEnumerable<string> d)
        {
            matching = m;
            different = d;
        }

        public IEnumerable<string> ListMatching()
        {
            return matching;
        }

        public IEnumerable<string> ListDifferent()
        {
            return different;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Files with same names but different data:\n");
            foreach (var diff in different)
                sb.Append($"{diff.ToString()}\n");

            sb.Append("Files with same names and same data:\n");
            foreach (var match in matching)
                sb.Append($"{match.ToString()}\n");
            return sb.ToString();
        }
    }

    public class Worker
    {
        public Worker()
        {

        }

        private static bool AreFilesEqual(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
                return false;

            var first_read_bytes = first.OpenRead();
            var second_read_bytes = second.OpenRead();

            var file1byte = first_read_bytes.ReadByte();
            var file2byte = second_read_bytes.ReadByte();

            while (file1byte != -1)
            {
                if (file1byte != file2byte)
                {
                    first_read_bytes.Close();
                    second_read_bytes.Close();
                    return false;
                }
                file1byte = first_read_bytes.ReadByte();
                file2byte = second_read_bytes.ReadByte();
            }

            return true;
        }

        public WorkerResult Run(string[] args)
        {
            if (args.Length != 2)
                throw new WorkerException("Invalid input data");

            try
            {
                DirectoryInfo firstdirtry = new DirectoryInfo(args[0]);
                DirectoryInfo seconddirtry = new DirectoryInfo(args[1]);
            }
            catch (Exception e)
            {
                throw new WorkerException("One or both directories are not valid");
            }

            DirectoryInfo firstdir = new DirectoryInfo(args[0]);
            DirectoryInfo seconddir = new DirectoryInfo(args[1]);


            if (!firstdir.Exists || !seconddir.Exists)
                throw new WorkerException("One or both directories do not exist");

            List<string> match = new List<string>();
            List<string> diff = new List<string>();

            IEnumerable<FileInfo> firstfiles = firstdir.EnumerateFiles();
            IEnumerable<FileInfo> secondfiles = seconddir.EnumerateFiles();

            foreach (FileInfo file1 in firstfiles)
            {
                foreach (FileInfo file2 in secondfiles)
                {
                    if (String.Compare(file1.Name, file2.Name, true) == 0)
                    {
                        if (file1.Length == file2.Length && AreFilesEqual(file1, file2))
                            match.Add(file1.Name);
                        else
                            diff.Add(file2.Name);
                    }
                }
            }

            match.OrderBy(s => s);
            diff.OrderBy(s => s);

            return new WorkerResult(match, diff);
        }

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Worker w = new Worker();
            try
            {
                var res = w.Run(args);
                Console.WriteLine(res.ToString());
            }
            catch (WorkerException e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}

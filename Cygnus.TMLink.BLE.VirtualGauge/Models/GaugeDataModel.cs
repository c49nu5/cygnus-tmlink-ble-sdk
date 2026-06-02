using Cygnus.TMLink.Protobuf.V1;
using System.Diagnostics;

namespace Cygnus.TMLink.BLE.VirtualGauge.Models
{
    internal class GaugeDataModel
    {
        private uint _liveMeasurementIndex;
        private uint _frozenMeasurementIndex;

        public GaugeDataModel()
        {
            UoM = Random.Shared.Next(2) == 1 ? Uom.Imperial : Uom.Metric;
            UTMode = Random.Shared.Next(2) == 1 ? UTMode.Me : UTMode.Se;
            CreateRecordList();
            CreateBscanList();
        }

        public Message.RecordList RecordList { get; private set; } = new();

        public List<Message.Record> Records { get; private set; } = [];

        public Dictionary<string, List<Message.RecordPoint>> RecordPoints { get; private set; } = [];

        public Message.BScanList BScanList { get; private set; } = new();

        public List<Message.BScan> BScans { get; private set; } = [];

        public Dictionary<string, List<Message.BScanPoint>> BScanPoints { get; private set; } = [];

        public int CurrentMeasurementIndex { get; set; }

        public Message.GaugeInfo GaugeInfo =>
            new Message.GaugeInfo
            {
                batteryLevel = Battery.Default.ChargeLevel < 1 && Battery.Default.ChargeLevel > 0 ? (uint)(Battery.Default.ChargeLevel * 100) : (uint)Math.Max(0, 100 - Stopwatch.Elapsed.TotalMinutes),
                serialNumber = SerialNumber,
                gaugeVariant = GaugeVariant.Plus,
                gaugeUD = 2,
                versionNumber = 1
            };

        public static uint SerialNumber { get; private set; } = (uint)Random.Shared.Next(13123, 3213213);

        public static Stopwatch Stopwatch { get; private set; } = Stopwatch.StartNew();
        public Uom UoM { get; }
        public UTMode UTMode { get; }

        private void CreateRecordList()
        {
            var recordCount = Random.Shared.Next(3, 10);
            Enumerable.Range(0, recordCount).ToList().ForEach(i =>
            {
                var type = Random.Shared.Next(2) == 1 ? RecordType.Linear : RecordType.Grid;
                uint colCount = (uint)Random.Shared.Next(1, 7);
                uint rowCount = (uint)Random.Shared.Next(1, 7);
                uint requiredPoints = type == RecordType.Grid ? colCount * rowCount : (uint)Random.Shared.Next(140) ;
                uint pointsTaken = (uint)(requiredPoints - Random.Shared.Next((int)requiredPoints));
                Message.RecordList.Item item = new()
                {
                    Name = $"{(type == RecordType.Linear ? "L" : "G")}R_{DateTime.Now.AddDays(- 1 - RecordList.Items.Count):ddMMMyy}_{DateTime.Now:HHmm}",
                    recordType = type,
                    numPointsRequired = requiredPoints,
                    numPointsTaken = pointsTaken,
                    Created = DateTime.Now.AddSeconds(-Random.Shared.Next(10000)),
                    Updated = DateTime.Now,
                    fileSize = requiredPoints * 150u,
                    Key = $"TML{(uint)DateTime.Now.TimeOfDay.TotalSeconds}"                    
                };
                RecordList.Items.Add(item);

                Records.Add(CreateRecord(item, colCount));
            });
        }

        internal Message.Record CreateRecord(Message.RecordList.Item record, uint colCount)
        {
            uint recordId = (uint)DateTime.Now.TimeOfDay.Seconds;
            var gaugeRecord = new Message.Record
            {
                Name = record.Name,
                recordType = record.recordType,
                numPointsRequired = record.numPointsRequired,
                numPointsTaken = record.numPointsTaken,
                Created = record.Created,
                Updated = record.Updated,
                recordID = recordId,
                Surveyor = "John Doe",
                Location = $"Pipeline {Random.Shared.Next(1, 15)}",               
                Key = record.Key
            };

            List<Message.RecordPoint> recordPoints = new List<Message.RecordPoint>();
            for (uint i = 0; i < record.numPointsTaken; i++)            
            {
                var point = CreateRecordPoint(i, record.recordType, colCount);
                recordPoints.Add(point);
            }

            RecordPoints[record.Name] = recordPoints;
            return gaugeRecord;
        }

        private Message.RecordPoint CreateRecordPoint(uint index, RecordType recordType, uint colCount)
        {
            var point = new Message.RecordPoint
            {
                Key = 232132 + index,
                colNumX = recordType == RecordType.Linear ? index : index % colCount,
                rowNumY = recordType == RecordType.Linear ? 0 : index / colCount,
                Method = Method.Scan,
                Uom = UoM,
                UTMode = UTMode,
                State = MeasurementPointState.Valid,
                probeType = ProbeType.Single,
                Taken = DateTime.Now.AddSeconds(-Random.Shared.Next(1000)),
                Thickness = (uint)Random.Shared.Next(45000),
                Velocity = (uint)Random.Shared.Next(1000, 9000),
                Ascan = new()
                {
                    ascanPoints = new byte[Random.Shared.Next(2) * 320]
                }
            };

            point.Name = recordType == RecordType.Linear ? $"P{index + 1}" : $"R{point.rowNumY + 1}.C{point.colNumX}";

            return point;
        }

        private void CreateBscanList()
        {
            var BscanCount = Random.Shared.Next(3, 10);
            Enumerable.Range(0, BscanCount).ToList().ForEach(i =>
            {
                uint requiredPoints = (uint)Random.Shared.Next(140, 232);
                uint pointsTaken = (uint)(requiredPoints - Random.Shared.Next((int)requiredPoints));
                Message.BScanList.Item item = new()
                {
                    Name = $"{DateTime.Now.AddDays(- 1 - BScanList.Items.Count):ddMMMyy}_{DateTime.Now:HHmm}",
                    numScanPoints = requiredPoints,
                    Updated = DateTime.Now,
                    fileSize = requiredPoints * 100u,
                    Key = $"TML{(uint)DateTime.Now.TimeOfDay.TotalSeconds}"
                };
                BScanList.Items.Add(item);

                BScans.Add(CreateBscan(item));
            });
        }

        internal Message.BScan CreateBscan(Message.BScanList.Item Bscan)
        {
            uint BscanId = (uint)DateTime.Now.TimeOfDay.TotalSeconds;
            var gaugeBscan = new Message.BScan
            {
                Name = Bscan.Name,
                numScanPoints = Bscan.numScanPoints,
                Updated = Bscan.Updated,
                BScanID = BscanId,
                Key = Bscan.Key
            };

            List<Message.BScanPoint> bscanPoints = new List<Message.BScanPoint>();
            for (uint i = 0; i < Bscan.numScanPoints; i++)
            {
                var point = CreateBscanPoint(i);
                bscanPoints.Add(point);
            }

            BScanPoints[Bscan.Name] = bscanPoints;
            return gaugeBscan;
        }

        private Message.BScanPoint CreateBscanPoint(uint index)
        {
            var point = new Message.BScanPoint
            {
                Uom = UoM,
                UTMode = UTMode,
                probeType = ProbeType.Single,
                Thickness = (uint)Random.Shared.Next(45000),
                Velocity = (uint)Random.Shared.Next(1000, 9000),
                Ascan = new()
                {
                    ascanPoints = new byte[Random.Shared.Next(2) * 320]
                }
            };

            return point;
        }

        internal void AddRecord(Protobuf.V1.Command.NewRecord record)
        {
            uint requiredPoints = record.recordType == RecordType.Linear ? record.numColsX : record.numColsX * record.numRowsY;
            DateTime createdTime = DateTime.Now;
            RecordList.Items.Add(new Message.RecordList.Item
            {
                Name = record.Name,
                recordType = record.recordType,
                numPointsRequired = requiredPoints,
                Created = createdTime,
                fileSize = requiredPoints * 150u,
                Key = record.Key
            });

            uint recordId = (uint)createdTime.TimeOfDay.Seconds;
            var gaugeRecord = new Message.Record
            {
                Name = record.Name,
                recordType = record.recordType,
                numPointsRequired = requiredPoints,
                Created = createdTime,
                recordID = recordId,
                Key = record.Key
            };
            Records.Add(gaugeRecord);
        }

        internal void AddRecordPoints(Protobuf.V1.Command.AddRecordPoints addRecordPoints)
        {
            var recordListItem = RecordList.Items.FirstOrDefault(r => r.Name == addRecordPoints.Name);
            if (recordListItem == null)
                return;

            recordListItem.Updated = DateTime.Now;

            var record = Records.FirstOrDefault(r => r.Name == addRecordPoints.Name);
            if (record == null)
                return;

            record.Updated = recordListItem.Updated;

            if (!RecordPoints.TryGetValue(addRecordPoints.Name, out var recordPoints))
            {
                recordPoints = [];
                RecordPoints[addRecordPoints.Name] = recordPoints;
            }

            recordPoints.AddRange(addRecordPoints.Mpoints.Select(p =>
            { 
                var point = new Message.RecordPoint
                {
                    Key = p.Key,
                    colNumX = p.colNumX,
                    rowNumY = p.rowNumY,
                    Method = p.Method,
                };
                point.Name = record.recordType == RecordType.Linear ? $"P{point.colNumX}" : $"R{point.rowNumY + 1}.C{point.colNumX}";
                switch(Random.Shared.Next(4))
                {
                    case 0:
                        record.numPointsTaken++;
                        point.Uom = UoM;
                        point.UTMode = UTMode;
                        point.probeType = ProbeType.Single;
                        point.Taken = DateTime.Now;
                        point.Thickness = (uint)Random.Shared.Next(1000, 45000);
                        point.Velocity = (uint)Random.Shared.Next(1000, 45000);
                        point.Ascan = new()
                        {
                            ascanPoints = Random.Shared.Next(2) == 1 ? new byte[320] : new byte[640]
                        };
                        point.State = MeasurementPointState.Valid;
                        break;
                    case 1:
                        point.State = MeasurementPointState.Blank;
                        break;
                    case 2:
                        point.State = MeasurementPointState.Obstructed;
                        break;
                    case 3:
                        point.State = MeasurementPointState.NoReading;
                        break;
                }
                return point;
            }).ToList());

            recordListItem.numPointsTaken = record.numPointsTaken;
        }

        public NotifyLiveMeasurement CreateLiveMeasurement(LiveMeasurementType measurementType, string liveThickness)
        {
            return new NotifyLiveMeasurement()
            {
                batteryLevel = GaugeInfo?.batteryLevel ?? 0,
                Thickness = double.TryParse(liveThickness, out var thickness) ? (uint)thickness : 0,
                gaindB = 20,
                Index = measurementType == LiveMeasurementType.Live ? _liveMeasurementIndex++ : _frozenMeasurementIndex++,
                liveMeasurementType = measurementType,
                statusBits = (uint)(measurementType == LiveMeasurementType.Live ? Constants.DeepcoatFlag | Constants.IsValidFlag : Constants.DeepcoatFlag | Constants.IsFrozenFlag | Constants.IsValidFlag),
                surfaceTemp = 25,
                UTMode = UTMode.Se,
                Velocity = 5900
            };
        }

        public FrozenLiveMeasurement CreateFrozenLiveMeasurement(string frozenThickness)
        {
            return new FrozenLiveMeasurement()
            {
                batteryLevel = GaugeInfo?.batteryLevel ?? 0,
                Thickness = double.TryParse(frozenThickness, out var thickness) ? (uint)thickness : 0,
                gaindB = 20,
                Index = _frozenMeasurementIndex,
                surfaceTemp = 25,
                UTMode = UTMode.Se,
                Velocity = 5900,
                Ascan = new AScan()
                {
                    ascanPoints = Enumerable.Range(0, 320).Select(i => (byte)(i % 256)).ToArray(),
                }
            };
        }

        public Message.RecordPoint? GetRecordPoint(string recordName, bool withAScans)
        {
            Message.RecordPoint? recordPoint = RecordPoints[recordName].Where(p => p.State == MeasurementPointState.Valid).ElementAtOrDefault(CurrentMeasurementIndex++);
            if (recordPoint == null)
                return null;

            if (!withAScans)
            {
                recordPoint.Ascan = null;
            }

            return recordPoint;
        }

        public Message.BScanPoint GetBScanPoint(string bScanName, bool withAScans)
        {
            Message.BScanPoint bScanPoint = BScanPoints[bScanName][CurrentMeasurementIndex++];
            if (!withAScans)
            {
                bScanPoint.Ascan = null;
            }

            return bScanPoint;
        }
    }
}

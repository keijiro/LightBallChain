using UnityEngine;
using UnityEngine.Timeline;

namespace LightBallChain
{
    [ExecuteInEditMode]
    public class ChainRenderer : MonoBehaviour, ITimeControl
    {
        #region Editable attributes

        enum MotionType {
            SyncedRandom, OrderedRandom,
            MonoLissajous, MultiLissajous,
            Longitude
        }
        [SerializeField] MotionType _motionType;

        [SerializeField] float _speed = 1;
        [SerializeField] float _interval = 1;
        [SerializeField] float _multiplier = 1;
        [SerializeField] int _randomSeed = 0;

        [SerializeField] float _radius = 1;
        [SerializeField] int _ballCount = 10;
        [SerializeField] float _ballScale = 1;

        [SerializeField, ColorUsage(false, true, 0, 8, 0.125f, 3)]
        Color _color = Color.white;

        #endregion

        #region Internal resources

        [SerializeField] Shader _lineShader;
        [SerializeField] Shader _ballShader;
        [SerializeField] Mesh _ballMesh;

        #endregion

        #region Private variables

        Material _lineMaterial;
        Material _ballMaterial;

        ComputeBuffer _positionBuffer;
        ComputeBuffer _drawArgsBuffer;

        Vector4 [] _positions;

        bool _underTimeControl;
        float _time;

        readonly Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        #endregion

        #region Local math functions

        // Hash function from H. Schechter & R. Bridson, goo.gl/RXiKaH
        static uint Hash(uint s)
        {
            s ^= 2747636419u;
            s *= 2654435769u;
            s ^= s >> 16;
            s *= 2654435769u;
            s ^= s >> 16;
            s *= 2654435769u;
            return s;
        }

        static float Random01(uint seed)
        {
            return Hash(seed) / 4294967295.0f; // 2^32-1
        }

        static float Random1(uint seed)
        {
            return Random01(seed) * 2 - 1;
        }

        static Vector3 RandomPointInCube(int seed)
        {
            return new Vector3(
                Random1((uint)(seed + 382943)),
                Random1((uint)(seed + 193893)),
                Random1((uint)(seed + 542194))
            );
        }

        static Vector3 RandomPointOnSphere(int seed)
        {
            var u = Random01((uint)(seed + 28913)) * Mathf.PI * 2;
            var z = Random01((uint)(seed + 92877)) * 2 - 1;
            var v = Mathf.Sqrt(1 - z * z);
            return new Vector3(Mathf.Cos(u) * v, Mathf.Sin(u) * v, z);
        }

        static float SmoothStep01(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3 - 2 * x);
        }

        #endregion

        #region Private methods

        void ReleaseComputeBuffers()
        {
            if (_positionBuffer != null)
            {
                _positionBuffer.Release();
                _drawArgsBuffer.Release();

                _positionBuffer = null;
                _drawArgsBuffer = null;
            }
        }

        void UpdatePositions()
        {
            switch (_motionType)
            {
                case MotionType.SyncedRandom: SyncedRandom(); break;
                case MotionType.OrderedRandom: OrderedRandom(); break;
                case MotionType.MonoLissajous: MonoLissajous(); break;
                case MotionType.MultiLissajous: MultiLissajous(); break;
                case MotionType.Longitude: Longitude(); break;
            }

            if (Application.isPlaying && !_underTimeControl)
                _time += Time.deltaTime;
            else
                _time = 0;
        }

        #endregion

        #region Animation functions

        void SyncedRandom()
        {
            var t = _time * _speed;
            var seed = _randomSeed + Mathf.FloorToInt(t) * _ballCount;
            var param = SmoothStep01(t - Mathf.Floor(t));

            for (var i = 0; i < _ballCount; i++)
            {
                var p1 = RandomPointOnSphere(seed + i);
                var p2 = RandomPointOnSphere(seed + i + _ballCount);
                _positions[i] = Vector3.Lerp(p1, p2, param);
            }
        }

        void OrderedRandom()
        {
            var t = _time * _speed;

            for (var i = 0; i < _ballCount; i++)
            {
                var seed = _randomSeed + Mathf.FloorToInt(t) * _ballCount;
                var param = SmoothStep01(t - Mathf.Floor(t));

                var p1 = RandomPointOnSphere(seed + i);
                var p2 = RandomPointOnSphere(seed + i + _ballCount);
                _positions[i] = Vector3.Lerp(p1, p2, param);

                t += 1.0f / _ballCount;
            }
        }

        void MonoLissajous()
        {
            var t = _time * _speed;
            var av = RandomPointInCube(_randomSeed) * 180;

            for (var i = 0; i < _ballCount; i++)
            {
                _positions[i] = Quaternion.Euler(av * t) * Vector3.up;
                t += _interval;
            }
        }

        void MultiLissajous()
        {
            var t = _time * _speed;

            for (var i = 0; i < _ballCount; i++)
            {
                var av = RandomPointInCube(_randomSeed + i * 371) * 180;
                _positions[i] = Quaternion.Euler(av * t) * Vector3.up;
            }
        }

        void Longitude()
        {
            var t = _time;

            for (var i = 0; i < _ballCount; i++)
            {
                var t1 = (t * _speed) % 1;
                var t2 = t * _speed * _multiplier * Mathf.PI * 2;

                float y, xz;
                if (t1 < 0.1f)
                {
                    y = 1 - SmoothStep01(t1 * 10) * 2;
                    xz = 0;
                }
                else
                {
                    y = (t1 - 0.1f) / 0.9f * 2 - 1;
                    xz = Mathf.Sqrt(1 - y * y);
                }

                _positions[i] = new Vector3(
                    Mathf.Cos(t2) * xz,
                    y,
                    Mathf.Sin(t2) * xz
                );

                t += _interval;
            }
        }

        #endregion

        #region MonoBehaviour methods

        void OnValidate()
        {
            _ballCount = Mathf.Max(_ballCount, 1);
            _ballScale = Mathf.Max(_ballScale, 0);
        }

        void OnDisable()
        {
            // Note: This should be done in OnDisable, not in OnDestroy.
            ReleaseComputeBuffers();
        }

        void OnDestroy()
        {
            if (_lineMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_lineMaterial);
                    Destroy(_ballMaterial);
                }
                else
                {
                    DestroyImmediate(_lineMaterial);
                    DestroyImmediate(_ballMaterial);
                }
            }
        }

        void Update()
        {
            // Destroy the internal buffers when the instance count was changed.
            if (_positions != null && _positions.Length != _ballCount)
            {
                ReleaseComputeBuffers();
                _positions = null;
            }

            // Lazy initialization of the compute buffers.
            if (_positionBuffer == null)
            {
                _positionBuffer = new ComputeBuffer(_ballCount, 4 * sizeof(float));
                _drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
                _drawArgsBuffer.SetData(new uint[5] { _ballMesh.GetIndexCount(0), (uint)_ballCount, 0, 0, 0 });
            }

            // Lazy initialization of the position buffer.
            if (_positions == null) _positions = new Vector4 [_ballCount];

            // Update the position buffer.
            UpdatePositions();
            _positionBuffer.SetData(_positions);

            // Lazy initialization of the materials.
            if (_lineMaterial == null)
            {
                _lineMaterial = new Material(_lineShader);
                _ballMaterial = new Material(_ballShader);
                _lineMaterial.hideFlags = HideFlags.DontSave;
                _ballMaterial.hideFlags = HideFlags.DontSave;
            }

            // Material parameters
            _lineMaterial.SetBuffer("_Positions", _positionBuffer);
            _ballMaterial.SetBuffer("_Positions", _positionBuffer);

            _lineMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
            _ballMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);

            _lineMaterial.SetColor("_Color", _color);
            _ballMaterial.SetColor("_Color", _color);

            _lineMaterial.SetFloat("_Radius", _radius);
            _ballMaterial.SetFloat("_Radius", _radius);

            _ballMaterial.SetFloat("_Scale", _ballScale);

            // Draw balls with an instanced indirect draw call.
            Graphics.DrawMeshInstancedIndirect(_ballMesh, 0, _ballMaterial, _bounds, _drawArgsBuffer);
        }

        void OnRenderObject()
        {
            // Draw lines with procedural draw.
            _lineMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Lines, 2 * (_ballCount - 1), 1);
        }

        #endregion

        #region ITimeControl implementation

        public void OnControlTimeStart()
        {
            _underTimeControl = true;
        }

        public void OnControlTimeStop()
        {
            _underTimeControl = false;
        }

        public void SetTime(double time)
        {
            _time = (float)time;
        }

        #endregion
    }
}

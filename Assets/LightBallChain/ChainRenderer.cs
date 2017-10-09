using UnityEngine;

namespace LightBallChain
{
    [ExecuteInEditMode]
    public class ChainRenderer : MonoBehaviour
    {
        #region Editable attributes

        enum MotionType { MonoLissajous, SyncedRandom }
        [SerializeField] MotionType _motionType;

        [SerializeField, ColorUsage(false, true, 0, 8, 0.125f, 3)]
        Color _color = Color.white;

        [SerializeField] float _radius = 1;
        [SerializeField] float _ballScale = 1;

        [SerializeField] int _instanceCount = 10;
        [SerializeField] float _frequency = 1;
        [SerializeField] float _interval = 1;

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
        float _time;

        Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        #endregion

        #region Private methods

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

        static Vector3 RandomPoint(int seed)
        {
            var u = Random01((uint)(seed + 28913)) * Mathf.PI * 2;
            var z = Random01((uint)(seed + 92877)) * 2 - 1;
            var v = Mathf.Sqrt(1 - z * z);
            return new Vector3(Mathf.Cos(u) * v, Mathf.Sin(u) * v, z);
        }

        void ReleaseComputeBuffers()
        {
            if (_positionBuffer != null)
            {
                _positionBuffer.Release();
                _drawArgsBuffer.Release();

                _positionBuffer = null;
                _drawArgsBuffer = null;
            }

            // To sync the lifetime of _positions to _positionBuffer.
            _positions = null;
        }

        void UpdatePositions()
        {
            switch (_motionType)
            {
                case MotionType.MonoLissajous: MonoLissajous(); break;
                case MotionType.SyncedRandom: SyncedRandom(); break;
            }

            if (Application.isPlaying)
                _time += _frequency * Time.deltaTime;
        }

        void MonoLissajous()
        {
            var t = _time;

            for (var i = 0; i < _instanceCount; i++)
            {
                _positions[i] = Quaternion.Euler(
                    58.158f * t, 183.24f * t, 36.442f * t
                ) * Vector3.up;

                t += _interval;
            }
        }

        void SyncedRandom()
        {
            var id = Mathf.FloorToInt(_time) * _instanceCount;
            var param = (_time - Mathf.Floor(_time));

            param = param * param * (3 - 2 * param);

            for (var i = 0; i < _instanceCount; i++)
            {
                var p1 = RandomPoint(id + i);
                var p2 = RandomPoint(id + i + _instanceCount);
                _positions[i] = Vector3.Lerp(p1, p2, param);
            }
        }

        #endregion

        #region MonoBehaviour methods

        void OnValidate()
        {
            _ballScale = Mathf.Max(_ballScale, 0);
            _instanceCount = Mathf.Max(_instanceCount, 1);
        }

        void OnDisable()
        {
            if (_positionBuffer != null) ReleaseComputeBuffers();
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
            // To reset the position buffer when the instance count was changed.
            if (_positions != null && _positions.Length != _instanceCount)
                ReleaseComputeBuffers();

            // Lazy initialization of the position buffer and the draw args buffer.
            if (_positionBuffer == null)
            {
                _positionBuffer = new ComputeBuffer(_instanceCount, 4 * sizeof(float));

                _drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
                _drawArgsBuffer.SetData(new uint[5] { _ballMesh.GetIndexCount(0), (uint)_instanceCount, 0, 0, 0 });

                _positions = new Vector4 [_instanceCount];
            }

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

            // Invoke instanced indirect draw of balls.
            Graphics.DrawMeshInstancedIndirect(_ballMesh, 0, _ballMaterial, _bounds, _drawArgsBuffer);
        }

        void OnRenderObject()
        {
            // Invoke procedural draw of lines.
            _lineMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Lines, 2 * (_instanceCount - 1), 1);
        }

        #endregion
    }
}

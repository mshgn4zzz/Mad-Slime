using System;
using System.Collections.Generic;
using UnityEngine;
namespace ShapeFill
{
    public class GridCameraFitter : MonoBehaviour
    {
        // Includes the completion pulse and leaves room for the fill UI.
        private const float FormPadding = 1.1f;
        private const float ViewportWidth = 0.84f;
        private const float ViewportHeight = 0.76f;

        [SerializeField] private Camera _camera;

        private Vector3 _formCenter;
        private readonly List<Vector3> _corners = new List<Vector3>();
        private float _fieldOfView;
        private float _aspect;
        private bool _hasForm;

        public void Fit(GridBuilder grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            Bounds bounds = grid.GetFormBounds();
            FitBounds(grid.transform.position, bounds.size, grid);
        }

        public void Fit(Vector3 formCenter, Vector3 formSize)
        {
            FitBounds(formCenter, formSize, null);
        }

        private void FitBounds(Vector3 formCenter, Vector3 formSize, GridBuilder grid)
        {
            if (_camera == null)
            {
                throw new InvalidOperationException($"{name}: Fill camera is not assigned.");
            }

            if (formSize.x <= 0f || formSize.y <= 0f || formSize.z <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(formSize), "Fit requires positive world-space bounds.");
            }

            _formCenter = formCenter;
            _fieldOfView = _camera.fieldOfView;
            _corners.Clear();
            _aspect = _camera.aspect;
            _hasForm = true;

            Vector3 halfSize = formSize * (0.5f * FormPadding);
            void AddCube(Vector3 offset, Vector3 extents)
            {
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        for (int z = -1; z <= 1; z += 2)
                        {
                            _corners.Add(offset + Vector3.Scale(extents, new Vector3(x, y, z)));
                        }
                    }
                }
            }

            if (grid == null)
            {
                AddCube(Vector3.zero, halfSize);
            }
            else
            {
                // Border cells contain every extreme of the occupied shape.
                foreach (Vector2Int cell in grid.BorderCells)
                    AddCube((grid.GridToWorld(cell.x, cell.y) - formCenter) * FormPadding,
                        Vector3.one * (grid.CellSize * 0.5f * FormPadding));
            }
            ApplyFit();
        }

        private void ApplyFit()
        {
            _aspect = _camera.aspect;
            Quaternion worldToCamera = Quaternion.Inverse(_camera.transform.rotation);
            float tanY = Mathf.Tan(_fieldOfView * Mathf.Deg2Rad * 0.5f);
            float tanX = tanY * _aspect;
            float minDepth = float.PositiveInfinity;
            float maxDepth = float.NegativeInfinity;
            float lowerX = float.NegativeInfinity, upperX = float.PositiveInfinity;
            float lowerY = float.NegativeInfinity, upperY = float.PositiveInfinity;
            float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
            float slopeX = tanX * ViewportWidth;
            float slopeY = tanY * ViewportHeight;

            foreach (Vector3 offset in _corners)
            {
                Vector3 corner = worldToCamera * offset;
                minDepth = Mathf.Min(minDepth, corner.z);
                maxDepth = Mathf.Max(maxDepth, corner.z);
                lowerX = Mathf.Max(lowerX, corner.x - slopeX * corner.z);
                upperX = Mathf.Min(upperX, corner.x + slopeX * corner.z);
                lowerY = Mathf.Max(lowerY, corner.y - slopeY * corner.z);
                upperY = Mathf.Min(upperY, corner.y + slopeY * corner.z);
                minX = Mathf.Min(minX, corner.x); maxX = Mathf.Max(maxX, corner.x);
                minY = Mathf.Min(minY, corner.y); maxY = Mathf.Max(maxY, corner.y);
            }

            // Each corner constrains an interval for the camera's horizontal/vertical offset.
            // The nearest distance is where those intervals first overlap on both axes.
            float distance = Mathf.Max(_camera.nearClipPlane + 0.01f - minDepth,
                (lowerX - upperX) / (2f * slopeX), (lowerY - upperY) / (2f * slopeY));
            float centerX = (lowerX + upperX) * 0.5f;
            float centerY = (lowerY + upperY) * 0.5f;
            if (_camera.orthographic)
            {
                _camera.orthographicSize = Mathf.Max((maxY - minY) / (2f * ViewportHeight),
                    (maxX - minX) / (2f * _aspect * ViewportWidth));
                distance = _camera.nearClipPlane + 0.01f - minDepth;
                centerX = (minX + maxX) * 0.5f;
                centerY = (minY + maxY) * 0.5f;
            }

            // Solve from the form and the view direction, never from the previous camera position.
            _camera.transform.position = _formCenter - _camera.transform.forward * distance
                + _camera.transform.right * centerX + _camera.transform.up * centerY;
            _camera.farClipPlane = Mathf.Max(_camera.farClipPlane, distance + maxDepth + 1f);
        }

        private void LateUpdate()
        {
            // Use the prepared geometry and FOV: a resize during the completion pulse/kick
            // must not bake the temporary animation values into the resting camera fit.
            if (_hasForm && Mathf.Approximately(_aspect, _camera.aspect) == false)
                ApplyFit();
        }
    }
}

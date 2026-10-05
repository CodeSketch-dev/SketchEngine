using UnityEngine;

namespace SketchEngine.Utilities.Motion
{
    public static class SpringMotion
    {
        // Video tham khảo: 
        // https://www.youtube.com/watch?v=bFOAipGJGA0
        // Hướng dẫn tạo cảm giác chơi tức thì - Giải thích cơ chế lò xo (của Toyful Games)

        // Thông báo bản quyền của mã nguồn gốc (giữ nguyên giấy phép):
        /******************************************************************************
        Copyright (c) 2008-2012 Ryan Juckett
        http://www.ryanjuckett.com/

        This software is provided 'as-is', without any express or implied
        warranty. In no event will the authors be held liable for any damages
        arising from the use of this software.

        Permission is granted to anyone to use this software for any purpose,
        including commercial applications, and to alter it and redistribute it
        freely, subject to the following restrictions:

        1. The origin of this software must not be misrepresented; you must not
            claim that you wrote the original software. If you use this software
            in a product, an acknowledgment in the product documentation would be
            appreciated but is not required.

        2. Altered source versions must be plainly marked as such, and must not be
            misrepresented as being the original software.

        3. This notice may not be removed or altered from any source
            distribution.
        ******************************************************************************/

        // File này sử dụng một số phần đã chỉnh sửa từ mã nguồn gốc. Các comment 
        // được dịch sang tiếng Việt; mã được chuyển từ
        // C++ sang C# bằng các phương thức Mathf của UnityEngine. Mã nguồn gốc nằm
        // trong bài viết: https://www.ryanjuckett.com/damped-springs/
        // Các quyền và điều kiện của giấy phép gốc được giữ nguyên. 


        //******************************************************************************
        // Bộ tham số chuyển động được tính sẵn để cập nhật hiệu quả
        // nhiều lò xo có cùng bước thời gian, tần số góc và
        // tỷ số cản.
        //******************************************************************************
        public struct DampedSpringMotionParams
        {
            // newPos = posPosCoef*oldPos + posVelCoef*oldVel
            public float posPosCoef, posVelCoef;
            // newVel = velPosCoef*oldPos + velVelCoef*oldVel
            public float velPosCoef, velVelCoef;
        };

        //******************************************************************************
        // Hàm này tính các tham số cần thiết để mô phỏng lò xo có lực cản
        // trong một khoảng thời gian nhất định.
        // - Tần số góc điều khiển tốc độ dao động của lò xo.
        // - Tỷ số cản điều khiển mức độ suy giảm của chuyển động.
        //     tỷ số cản > 1: quá cản (trở về cân bằng mà không dao động)
        //     tỷ số cản = 1: cản tới hạn (trở về cân bằng nhanh nhất mà không dao động)
        //     tỷ số cản < 1: dưới cản (dao động quanh vị trí cân bằng)
        //******************************************************************************
        static DampedSpringMotionParams CalcDampedSpringMotionParams(
            float deltaTime,        // bước thời gian cần mô phỏng
            float angularFrequency, // tần số góc của chuyển động
            float dampingRatio)     // tỷ số cản của chuyển động
        {
            const float epsilon = 0.0001f;
            DampedSpringMotionParams pOutParams;

            // Giới hạn các giá trị trong phạm vi hợp lệ
            if (dampingRatio < 0.0f) dampingRatio = 0.0f;
            if (angularFrequency < 0.0f) angularFrequency = 0.0f;

            // Nếu tần số góc gần bằng 0, giữ nguyên vị trí và vận tốc bằng cách
            // trả về các hệ số của phép biến đổi đồng nhất
            if (angularFrequency < epsilon)
            {
                pOutParams.posPosCoef = 1.0f; pOutParams.posVelCoef = 0.0f;
                pOutParams.velPosCoef = 0.0f; pOutParams.velVelCoef = 1.0f;
                return pOutParams;
            }

            if (dampingRatio > 1.0f + epsilon)
            {
                // Quá cản
                float za = -angularFrequency * dampingRatio;
                float zb = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1.0f);
                float z1 = za - zb;
                float z2 = za + zb;
                // Tính lũy thừa của số e (xấp xỉ 2,718)
                float e1 = Mathf.Exp(z1 * deltaTime);
                float e2 = Mathf.Exp(z2 * deltaTime);

                float invTwoZb = 1.0f / (2.0f * zb); // = 1 / (z2 - z1)

                float e1_Over_TwoZb = e1 * invTwoZb;
                float e2_Over_TwoZb = e2 * invTwoZb;

                float z1e1_Over_TwoZb = z1 * e1_Over_TwoZb;
                float z2e2_Over_TwoZb = z2 * e2_Over_TwoZb;

                pOutParams.posPosCoef = e1_Over_TwoZb * z2 - z2e2_Over_TwoZb + e2;
                pOutParams.posVelCoef = -e1_Over_TwoZb + e2_Over_TwoZb;

                pOutParams.velPosCoef = (z1e1_Over_TwoZb - z2e2_Over_TwoZb + e2) * z2;
                pOutParams.velVelCoef = -z1e1_Over_TwoZb + z2e2_Over_TwoZb;
            }
            else if (dampingRatio < 1.0f - epsilon)
            {
                // Dưới cản
                float omegaZeta = angularFrequency * dampingRatio;
                float alpha = angularFrequency * Mathf.Sqrt(1.0f - dampingRatio * dampingRatio);

                float expTerm = Mathf.Exp(-omegaZeta * deltaTime);
                float cosTerm = Mathf.Cos(alpha * deltaTime);
                float sinTerm = Mathf.Sin(alpha * deltaTime);

                float invAlpha = 1.0f / alpha;

                float expSin = expTerm * sinTerm;
                float expCos = expTerm * cosTerm;
                float expOmegaZetaSin_Over_Alpha = expTerm * omegaZeta * sinTerm * invAlpha;

                pOutParams.posPosCoef = expCos + expOmegaZetaSin_Over_Alpha;
                pOutParams.posVelCoef = expSin * invAlpha;

                pOutParams.velPosCoef = -expSin * alpha - omegaZeta * expOmegaZetaSin_Over_Alpha;
                pOutParams.velVelCoef = expCos - expOmegaZetaSin_Over_Alpha;
            }
            else
            {
                // Cản tới hạn
                float expTerm = Mathf.Exp(-angularFrequency * deltaTime);
                float timeExp = deltaTime * expTerm;
                float timeExpFreq = timeExp * angularFrequency;

                pOutParams.posPosCoef = timeExpFreq + expTerm;
                pOutParams.posVelCoef = timeExp;

                pOutParams.velPosCoef = -angularFrequency * timeExpFreq;
                pOutParams.velVelCoef = -timeExpFreq + expTerm;
            }
            return pOutParams;
        }

        //******************************************************************************
        // Hàm này cập nhật vị trí và vận tốc được truyền vào
        // theo các tham số chuyển động.
        //******************************************************************************
        static void UpdateDampedSpringMotion(
            ref float pPos,           // giá trị vị trí cần cập nhật
            ref float pVel,           // giá trị vận tốc cần cập nhật
            float equilibriumPos, // vị trí cân bằng cần hướng tới
            DampedSpringMotionParams parameters)         // các tham số chuyển động được sử dụng
        {
            float oldPos = pPos - equilibriumPos; // Tính trong hệ tọa độ tương đối so với vị trí cân bằng
            float oldVel = pVel;

            pPos = oldPos * parameters.posPosCoef + oldVel * parameters.posVelCoef + equilibriumPos;
            pVel = oldPos * parameters.velPosCoef + oldVel * parameters.velVelCoef;
        }

        /// <summary>
        /// Cập nhật chuyển động lò xo trong khoảng thời gian deltaTime
        /// </summary>
        /// <param name="position">Giá trị vị trí hiện tại, được cập nhật trực tiếp</param>
        /// <param name="velocity">Giá trị vận tốc hiện tại, được cập nhật trực tiếp</param>
        /// <param name="equilibriumPosition">Vị trí mục tiêu (hoặc vị trí cân bằng)</param>
        /// <param name="deltaTime">Khoảng thời gian cần mô phỏng</param>
        /// <param name="angularFrequency">Tần số góc của chuyển động</param>
        /// <param name="dampingRatio">Tỷ số cản của chuyển động</param>
        public static void CalcDampedSimpleHarmonicMotion(ref float position, ref float velocity,
            float equilibriumPosition, float deltaTime, float angularFrequency, float dampingRatio)
        {
            var motionParams = CalcDampedSpringMotionParams(deltaTime, angularFrequency, dampingRatio);
            UpdateDampedSpringMotion(ref position, ref velocity, equilibriumPosition, motionParams);
        }

        /// <summary>
        /// Cập nhật chuyển động lò xo trong khoảng thời gian deltaTime
        /// </summary>
        /// <param name="position">Giá trị vị trí hiện tại, được cập nhật trực tiếp</param>
        /// <param name="velocity">Giá trị vận tốc hiện tại, được cập nhật trực tiếp</param>
        /// <param name="equilibriumPosition">Vị trí mục tiêu (hoặc vị trí cân bằng)</param>
        /// <param name="deltaTime">Khoảng thời gian cần mô phỏng</param>
        /// <param name="angularFrequency">Tần số góc của chuyển động</param>
        /// <param name="dampingRatio">Tỷ số cản của chuyển động</param>
        public static void CalcDampedSimpleHarmonicMotion(ref Vector2 position, ref Vector2 velocity,
            Vector2 equilibriumPosition, float deltaTime, float angularFrequency, float dampingRatio)
        {
            var motionParams = CalcDampedSpringMotionParams(deltaTime, angularFrequency, dampingRatio);
            UpdateDampedSpringMotion(ref position.x, ref velocity.x, equilibriumPosition.x, motionParams);
            UpdateDampedSpringMotion(ref position.y, ref velocity.y, equilibriumPosition.y, motionParams);
        }

        /// <summary>
        /// Cập nhật chuyển động lò xo trong khoảng thời gian deltaTime
        /// </summary>
        /// <param name="position">Giá trị vị trí hiện tại, được cập nhật trực tiếp</param>
        /// <param name="velocity">Giá trị vận tốc hiện tại, được cập nhật trực tiếp</param>
        /// <param name="equilibriumPosition">Vị trí mục tiêu (hoặc vị trí cân bằng)</param>
        /// <param name="deltaTime">Khoảng thời gian cần mô phỏng</param>
        /// <param name="angularFrequency">Tần số góc của chuyển động</param>
        /// <param name="dampingRatio">Tỷ số cản của chuyển động</param>
        public static void CalcDampedSimpleHarmonicMotion(ref Vector3 position, ref Vector3 velocity,
            Vector3 equilibriumPosition, float deltaTime, float angularFrequency, float dampingRatio)
        {
            var motionParams = CalcDampedSpringMotionParams(deltaTime, angularFrequency, dampingRatio);
            UpdateDampedSpringMotion(ref position.x, ref velocity.x, equilibriumPosition.x, motionParams);
            UpdateDampedSpringMotion(ref position.y, ref velocity.y, equilibriumPosition.y, motionParams);
            UpdateDampedSpringMotion(ref position.z, ref velocity.z, equilibriumPosition.z, motionParams);
        }
    }
}
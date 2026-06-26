using System;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.PlannedStudy
{
    public sealed class RandomBodyPartDataRule : IGeneratorRule<(string bodyPartGuid, string anatomicCode, string description, string viewCode, string lateralityCode)>
    {
        private static readonly Random _random = new Random();

        private (string bodyPartGuid, string anatomicCode, string description, string viewCode, string lateralityCode)[] GetBodyPartDataArray()
        {
            return new (string, string, string, string, string)[]
            {               
                // 1. Грудная клетка - Бронхография (Задняя)
                ("7b3ec16c-ed90-40a3-908f-0480ef626137", "955009", "Chest - Bronchus (PA)", "272479007", "U"),
        
                // 2. Грудная клетка - Грудина
                ("7b3ec16c-ed90-40a3-908f-0480ef626137", "56873002", "Chest - Sternum", "399348003", "U"),
        
                // 3. Голова - Верхняя челюсть
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "70925003", "Head - Maxilla", "399348003", "U"),
        
                // 4. Голова - Верхняя челюсть (Задняя)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "70925003", "Head - Maxilla (AP)", "399348003", "U"),
        
                // 5. Голова - ВНЧС
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "53620006", "Head - TMJ", "399348003", "P"),
        
                // 6. Голова - ВНЧС (Задняя)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "53620006", "Head - TMJ (PA)", "272479007", "P"),
        
                // 7. Голова - ВНЧС (Боковая)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "53620006", "Head - TMJ (Lateral)", "399067008", "P"),
        
                // 8. Голова - ВНЧС (Косая)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "53620006", "Head - TMJ (Oblique)", "399182000", "P"),
        
                // 9. Голова - Височные кости (Косая)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "89546000", "Head - Temporal bones (Oblique)", "399182000", "P"),
        
                // 10. Голова - Височные кости (Фронтально-косая аксиальная)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "89546000", "Head - Temporal bones (Frontal-oblique axial)", "399132005", "P"),
        
                // 11. Голова - Височные кости (Боковая косая)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "89546000", "Head - Temporal bones (Lateral oblique)", "260427002", "P"),
        
                // 12. Голова - Глазница
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "363654007", "Head - Orbital structure", "399348003", "P"),
        
                // 13. Голова - Кости носа
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "74386004", "Head - Nasal bone", "399348003", "U"),
        
                // 14. Голова - Кости носа (Боковая)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "74386004", "Head - Nasal bone (Lateral)", "399067008", "U"),
        
                // 15 Голова - Нижняя челюсть
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "91609006", "Head - Mandible", "399348003", "U"),
        
                // 16. Голова - Нижняя челюсть (Задняя)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "91609006", "Head - Mandible (PA)", "272479007", "U"),
        
                // 17. Голова - Нижняя челюсть (Осевая)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "91609006", "Head - Mandible (Axial)", "399061009", "U"),
        
                // 18. Голова - Носоглотка
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "661005", "Head - Jaw region", "399348003", "U"),
        
                // 21. Голова - Придаточные пазухи носа
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "2095001", "Head - Paranasal sinus", "399348003", "U"),
        
                // 22. Голова - Придаточные пазухи носа (Задняя)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "2095001", "Head - Paranasal sinus (PA)", "272479007", "U"),
        
                // 23. Голова - Придаточные пазухи носа (Боковая)
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "2095001", "Head - Paranasal sinus (Lateral)", "399067008", "U"),
        
                // 24. Голова - Скуловая дуга
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "13881006", "Head - Zygomatic arch", "399182000", "P"),
        
                // 25. Голова - Турецкое седло
                ("dc5133f5-6412-4c86-a307-0b682abf62d7", "42575006", "Head - Sella turcica", "399067008", "U"),
        
                // 30. Верхние конечности - Плечевой сустав
                ("075078f3-a940-492f-8874-67c989d1b36a", "16982005", "Upper limbs - Shoulder", "399033003", "B"),
        
                // 31. Верхние конечности - Плечевой сустав (Задняя)
                ("075078f3-a940-492f-8874-67c989d1b36a", "16982005", "Upper limbs - Shoulder (PA)", "272479007", "P"),
        
                // 32. Верхние конечности - Плечевой сустав (Осевая)
                ("075078f3-a940-492f-8874-67c989d1b36a", "16982005", "Upper limbs - Shoulder (Axial)", "399061009", "P"),
        
                // 33. Верхние конечности - Плечевой сустав (Боковая)
                ("075078f3-a940-492f-8874-67c989d1b36a", "16982005", "Upper limbs - Shoulder (Lateral)", "399067008", "P"),
        
                // 34. Верхние конечности - Плечо
                ("075078f3-a940-492f-8874-67c989d1b36a", "85050009", "Upper limbs - Humerus", "399033003", "B"),
        
                // 35. Верхние конечности - Плечо (Задняя)
                ("075078f3-a940-492f-8874-67c989d1b36a", "85050009", "Upper limbs - Humerus (PA)", "272479007", "P"),
        
                // 36. Верхние конечности - Плечо (Трансторакальная боковая)
                ("075078f3-a940-492f-8874-67c989d1b36a", "85050009", "Upper limbs - Humerus (Medial-lateral)", "399260004", "P"),
        
                // 37. Верхние конечности - Локтевой сустав
                ("075078f3-a940-492f-8874-67c989d1b36a", "16953009", "Upper limbs - Elbow joint", "399033003", "B"),
        
                // 38. Верхние конечности - Локтевой сустав (Медиально-латеральная)
                ("075078f3-a940-492f-8874-67c989d1b36a", "16953009", "Upper limbs - Elbow joint (Medial-lateral)", "399260004", "P"),
        
                // 39. Верхние конечности - Предплечье
                ("075078f3-a940-492f-8874-67c989d1b36a", "14975008", "Upper limbs - Forearm bone", "399033003", "B"),
        
                // 40. Верхние конечности - Лучезапястный сустав
                ("075078f3-a940-492f-8874-67c989d1b36a", "74670003", "Upper limbs - Wrist joint", "399033003", "B"),
        
                // 41. Верхние конечности - Кисть
                ("075078f3-a940-492f-8874-67c989d1b36a", "85562004", "Upper limbs - Wrist", "399033003", "B"),
        
                // 42. Верхние конечности - Кисть пальцы
                ("075078f3-a940-492f-8874-67c989d1b36a", "7569003", "Upper limbs - Wrist fingers", "399033003", "B"),
        
                // 43. Шея - Гортань
                ("f3f8758f-e7bc-4e60-8591-c9a08a371016", "4596009", "Neck - Larynx", "399348003", "U"),
        
                // 44. Шея - Гортань (Задняя)
                ("f3f8758f-e7bc-4e60-8591-c9a08a371016", "4596009", "Neck - Larynx (PA)", "272479007", "U"),
        
                // 45. Шея - Гортань (Боковая)
                ("f3f8758f-e7bc-4e60-8591-c9a08a371016", "4596009", "Neck - Larynx (Lateral)", "399067008", "U"),
        
                // 46. Нижние конечности - Бедро
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "71341001", "Lower limbs - Femur", "399033003", "B"),
        
                // 47. Нижние конечности - Бедро (Задняя)
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "71341001", "Lower limbs - Femur (PA)", "272479007", "P"),
        
                // 48. Нижние конечности - Бедро (Боковая)
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "71341001", "Lower limbs - Femur (Lateral)", "399067008", "P"),
        
                // 49. Нижние конечности - Коленный сустав
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "72696002", "Lower limbs - Knee joint", "399033003", "B"),
        
                // 50. Нижние конечности - Надколенник
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "64234005", "Lower limbs - Patella", "399348003", "B"),
        
                // 51. Нижние конечности - Голень
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "30021000", "Lower limbs - Leg", "399033003", "B"),
        
                // 52. Нижние конечности - Голеностопный сустав
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "70258002", "Lower limbs - Ancle joint", "399033003", "B"),
        
                // 53. Нижние конечности - Пяточная кость
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "80144004", "Lower limbs - Calcaneus", "399067008", "P"),
        
                // 54. Нижние конечности - Стопа
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "56459004", "Lower limbs - Foot", "399033003", "B"),
        
                // 55. Нижние конечности - Стопа с нагрузкой
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "56459004", "Lower limbs - Foot (load)", "399348003", "B"),
        
                // 56. Нижние конечности - Стопа пальцы
                ("fd1853ce-96ad-4de2-b7b1-8e606a1f5aac", "29707007", "Lower limbs - Toe", "272479007", "P"),
            };
        }

        public (string bodyPartGuid, string anatomicCode, string description, string viewCode, string lateralityCode) Generate()
        {
            var bodyParts = GetBodyPartDataArray();
            return bodyParts[_random.Next(bodyParts.Length)];
        }
    }
}

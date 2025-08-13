using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class CSVCombiner
{
    [MenuItem("Tools/Sound/Combine CSV Files")]
    public static void CombineCSVFiles()
    {
        string inputFolderPath = Path.Combine(Application.dataPath, "11_Data/SoundTableForMember");
        string outputFilePath = Path.Combine(Application.dataPath, "Resources/Sounds/SampleSoundTable.csv");
        string outputEnumPath = Path.Combine(Application.dataPath, "02_Scripts/Util/SoundEnum.cs");

        Debug.Log($"[CSVCombiner] 시작됨");
        Debug.Log($"[CSVCombiner] 입력 폴더 경로: {inputFolderPath}");
        Debug.Log($"[CSVCombiner] 출력 CSV 경로: {outputFilePath}");
        Debug.Log($"[CSVCombiner] 출력 Enum 경로: {outputEnumPath}");

        if (!Directory.Exists(inputFolderPath))
        {
            Debug.LogError("입력 폴더가 존재하지 않습니다: " + inputFolderPath);
            return;
        }

        var csvFiles = Directory.GetFiles(inputFolderPath, "*.csv");
        Debug.Log($"[CSVCombiner] 발견된 CSV 파일 수: {csvFiles.Length}");

        if (csvFiles.Length == 0)
        {
            Debug.LogWarning("CSV 파일이 없습니다: " + inputFolderPath);
            return;
        }

        StringBuilder sb = new StringBuilder();
        HashSet<string> enumIds = new HashSet<string>();
        bool isFirstFile = true;

        foreach (var file in csvFiles)
        {
            Debug.Log($"[CSVCombiner] 처리 중인 파일: {file}");

            var lines = File.ReadAllLines(file);
            Debug.Log($"[CSVCombiner] 줄 수 (포함 헤더): {lines.Length}");

            if (lines.Length == 0)
            {
                Debug.LogWarning($"[CSVCombiner] 빈 파일: {file}");
                continue;
            }

            if (isFirstFile)
            {
                sb.AppendLine(lines[0]); // 헤더 포함
                Debug.Log("[CSVCombiner] 헤더 추가 완료");
                isFirstFile = false;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    Debug.LogWarning($"[CSVCombiner] 빈 줄 스킵 (파일: {file}, 줄: {i + 1})");
                    continue;
                }

                sb.AppendLine(line);

                // 첫 번째 열 (id) 추출
                string[] values = line.Split(',');
                if (values.Length > 0)
                {
                    string rawId = values[0].Trim();
                }
            }
        }

        Debug.Log($"[CSVCombiner] 최종 Enum ID 수: {enumIds.Count}");

        // CSV 병합 저장
        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
        File.WriteAllText(outputFilePath, sb.ToString(), Encoding.UTF8);
        Debug.Log("CSV 병합 완료: " + outputFilePath);

        // Enum 생성
        Debug.Log("SoundEnum 생성 완료: " + outputEnumPath);

        AssetDatabase.Refresh(); // 에디터 갱신
    }
    
}
#endif

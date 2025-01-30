using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[ExecuteAlways] // Editor繧貞��逕溘＠縺ｦ縺��↑縺上※繧ょｮ溯｡後〒縺阪ｋ
public class AspectKeeper : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera; //蟇ｾ雎｡縺ｨ縺吶ｋ繧ｫ繝｡繝ｩ

    [SerializeField]
    private Vector2 aspectVec; //逶ｮ逧��ｧ｣蜒丞ｺｦ

    void Update()
    {
        var screenAspect = Screen.width / (float)Screen.height; //逕ｻ髱｢縺ｮ繧｢繧ｹ繝壹け繝域ｯ�
        var targetAspect = aspectVec.x / aspectVec.y; //逶ｮ逧�の繧｢繧ｹ繝壹け繝域ｯ�

        var magRate = targetAspect / screenAspect; //逶ｮ逧��い繧ｹ繝壹け繝域ｯ斐↓縺吶ｋ縺溘ａ縺ｮ蛟咲紫

        var viewportRect = new Rect(0, 0, 1, 1); //Viewport蛻晄悄蛟､縺ｧRect繧剃ｽ懈��

        if (magRate < 1)
        {
            viewportRect.width = magRate; //菴ｿ逕ｨ縺吶ｋ讓ｪ蟷��ｒ螟画峩
            viewportRect.x = 0.5f - viewportRect.width * 0.5f;//荳ｭ螟ｮ蟇��○
        }
        else
        {
            viewportRect.height = 1 / magRate; //菴ｿ逕ｨ縺吶ｋ邵ｦ蟷��ｒ螟画峩
            viewportRect.y = 0.5f - viewportRect.height * 0.5f;//荳ｭ螟ｮ菴咏函
        }

        targetCamera.rect = viewportRect; //繧ｫ繝｡繝ｩ縺ｮViewport縺ｫ驕ｩ逕ｨ
    }
}

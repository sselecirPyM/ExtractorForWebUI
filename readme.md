# Extractor for WebUI

## 功能
* 读取任务列表并执行

* 自动向指定的URL请求Stable Diffusion WebUI生成图片

* 通过SSH连接访问远程资源

* 将请求的资源保存在本地

## 如何使用

1. 如果要通过SSH访问远程资源，需要配置ssh服务器免密登录。

2. 确保服务器工作正常。Stable Diffusion WebUI处于运行状态。

3. 编写服务器列表appconfig.json和任务列表tasks.json并放置在软件目录下。

4. 运行程序，程序会从任务中选择一项执行。当前版本所有任务执行完毕后不会退出。


## 示例
appconfig.json
```json
{
    "WebUIServers": [
        {
            "Name": "RemoteGPU1",
            "Activate": false,
            "SSHConfig": {
                "Port": 22,
                "HostName": "192.168.1.1",
                "UserName": "root",
                "Forwarding": 7860,
                "LocalPort": 7861
            }
        },
        {
            "Name": "RemoteGPU2",
            "BatchSize": 4,
            "SSHConfig": {
                "Port": 1234,
                "HostName": ".com",
                "UserName": "root",
                "Forwarding": 6006
            }
        },
        {
            "Name": "localhost",
            "BatchSize": 2,
            "URL": "http://127.0.0.1:7860"
        }
    ],
    "PrivateKeyFile": "%USERPROFILE%\\.ssh\\id_rsa"
}
```

tasks.json
```json
{
    "requests": [
        {
            "saveDirectory": "images/character",
            "imageCount": 5,
            "prompt": "masterpiece, best quality, light pink hair, yellow eyes, cat ear, skirt,",
            "negativePrompt": "nsfw, lowres, bad anatomy, bad hands, text, error, missing fingers, extra digit, fewer digits, cropped, worst quality, low quality, normal quality, jpeg artifacts, signature, watermark, username, blurry, ugly, 3D game, bad art, bad shadow, long neck, liquid body, liquid tongue, font,",
            "step": 28,
            "sampleMethod": "Euler a",
            "cfgScale": 12,
            "height": 512,
            "width": 512,
            "seed": -1,
            "subSeed": -1,
            "subSeedStrength": 0,
            "denoiseStrenth": 0.7
        }
    ]
}
```


<?php

namespace Database\Seeders;

use App\Models\NGWord;
use GuzzleHttp\Promise\Create;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Storage;

class NGWordTableSeeder extends Seeder
{
    public function run(): void
    {
        // テキストファイルのパス
        $filePath = 'txt/NGWord.txt';

        // Storageファザードを使って読み込む
        $content = Storage::get($filePath);

        // 区切り文字で分割する(改行指定)
        $lines = explode(PHP_EOL, $content);

        foreach ($lines as $line) {
            // 改行を削除して登録する
            NGWord::create([
                'word' => str_replace([PHP_EOL, " "], "", $line)
            ]);
        }
    }
}

<?php

namespace Database\Seeders;

use App\Models\User;
use Illuminate\Database\Seeder;

class UserTableSeeder extends Seeder
{
    public function run(): void
    {
        // データを挿入
        User::create([
            'name' => 'ひよこ',
            'token' => 'A',
            'character_id' => 1
        ]);
        User::create([
            'name' => 'チキンひよこ',
            'token' => 'B',
            'character_id' => 2
        ]);
        User::create([
            'name' => 'ブラックひよこ',
            'token' => 'C',
            'character_id' => 3
        ]);
        User::create([
            'name' => 'スターひよこ',
            'token' => 'D',
            'character_id' => 4
        ]);
        User::create([
            'name' => 'ゴールドひよこ',
            'token' => 'E',
            'character_id' => 5
        ]);
    }
}

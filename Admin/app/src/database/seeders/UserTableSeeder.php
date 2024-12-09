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
            'name' => '1番さん',
            'token' => 'A',
            'character_id' => 1
        ]);
        User::create([
            'name' => '2番ちゃん',
            'token' => 'B',
            'character_id' => 1
        ]);
        User::create([
            'name' => '3番野郎',
            'token' => 'C',
            'character_id' => 1
        ]);
        User::create([
            'name' => '4番くん',
            'token' => 'D',
            'character_id' => 1
        ]);
    }
}

<?php

namespace Database\Seeders;

use App\Models\Rating;
use Illuminate\Database\Seeder;

class RatingsTableSeeder extends Seeder
{
    public function run(): void
    {
        // データを挿入
        Rating::create([
            'user_id' => '1',
            'rating' => random_int(0, 1500),
        ]);
        // データを挿入
        Rating::create([
            'user_id' => '2',
            'rating' => random_int(0, 1500),
        ]);
        // データを挿入
        Rating::create([
            'user_id' => '3',
            'rating' => random_int(0, 1500),
        ]);
        // データを挿入
        Rating::create([
            'user_id' => '4',
            'rating' => random_int(0, 1500),
        ]);
        // データを挿入
        Rating::create([
            'user_id' => '5',
            'rating' => random_int(0, 1500),
        ]);
    }
}

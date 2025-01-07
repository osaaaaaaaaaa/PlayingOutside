<?php

namespace Database\Seeders;

use App\Models\Follow;
use Illuminate\Database\Seeder;

class FollowsTableSeeder extends Seeder
{
    public function run(): void
    {
        // データを挿入
        Follow::create([
            'following_id' => '1',
            'followee_id' => '2',
        ]);
        Follow::create([
            'following_id' => '1',
            'followee_id' => '3',
        ]);
        Follow::create([
            'following_id' => '2',
            'followee_id' => '1',
        ]);
        Follow::create([
            'following_id' => '3',
            'followee_id' => '2',
        ]);
        // データを挿入
        Follow::create([
            'following_id' => '4',
            'followee_id' => '1',
        ]);
    }
}
